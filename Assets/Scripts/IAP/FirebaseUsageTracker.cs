using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;
using TMPro;

[System.Serializable]
public class UserSession
{
    public string userId;
    public string deviceId;
    public long sessionStart;
    public int dailyUsageMinutes;
    public long lastActivity;
    public bool isActive;
    public string deviceName;
}

[System.Serializable]
public class SessionResponse
{
    public bool isActiveElsewhere;
    public int dailyUsageMinutes;
    public string activeDeviceName;
}

/// <summary>
/// Firebase-based usage tracker for Meta Quest applications
/// Handles session management, device conflict detection, and daily usage limits
/// </summary>
public class FirebaseUsageTracker : MonoBehaviour
{
    [Header("Firebase Configuration")]
    public string firebaseUrl = "https://durlabh-darshan-oculus-default-rtdb.firebaseio.com";
    public string firebaseSecret = ""; // Optional for public read/write

    [Header("Usage Settings")]
    public int dailyUsageLimitMinutes = 120; // 2 hours default limit
    public int sessionUpdateIntervalSeconds = 30; // How often to update Firebase
    public int timeBudgetUpdateMinutes = 1; // How many minutes to add per update

    [Header("Events")]
    public UnityEngine.Events.UnityEvent OnSessionStarted;
    public UnityEngine.Events.UnityEvent OnSessionEnded;
    public UnityEngine.Events.UnityEvent OnUsageLimitReached;
    public UnityEngine.Events.UnityEvent OnDeviceConflict;

    // Private variables
    private string userId = "Test"; // Will be replaced with actual Meta Quest ID
    private string deviceId;
    private DateTime sessionStart;
    private Coroutine sessionUpdateCoroutine;
    private bool isSessionActive = false;
    private int currentSessionMinutes = 0;

    #region Unity Lifecycle

    void Start()
    {
        InitializeUserTracking();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            EndSession();
        }
        else if (!isSessionActive)
        {
            StartCoroutine(CheckAndStartSession());
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            EndSession();
        }
    }

    void OnDestroy()
    {
        EndSession();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize user tracking system
    /// Gets user ID and device ID, then checks for existing sessions
    /// </summary>
    void InitializeUserTracking()
    {
        GetMetaQuestUserId();
        deviceId = SystemInfo.deviceUniqueIdentifier;
        StartCoroutine(CheckAndStartSession());
    }

    /// <summary>
    /// Get Meta Quest user ID using Oculus Platform SDK
    /// Falls back to test ID in editor or if SDK fails
    /// </summary>
    void GetMetaQuestUserId()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Oculus.Platform.Users.GetLoggedInUser().OnComplete(message =>
        {
            if (!message.IsError)
            {
                userId = message.Data.ID.ToString();
                Debug.Log($"Meta Quest User ID obtained: {userId}");
            }
            else
            {
                Debug.LogError("Failed to get Meta Quest User ID: " + message.GetError().Message);
                userId = "fallback_" + UnityEngine.Random.Range(1000, 9999);
            }
        });
#else
        // Keep "Test" for editor testing
        Debug.Log("Using test user ID in editor mode");
#endif
    }

    #endregion

    #region Public Session Management Methods

    /// <summary>
    /// Manually start a new session
    /// Checks for conflicts and usage limits before starting
    /// </summary>
    public void StartSession()
    {
        if (isSessionActive)
        {
            Debug.LogWarning("Session is already active");
            return;
        }

        StartCoroutine(CheckAndStartSession());
    }

    /// <summary>
    /// Manually end the current session
    /// Updates Firebase and stops all tracking coroutines
    /// </summary>
    public void EndSession()
    {
        if (!isSessionActive)
        {
            Debug.LogWarning("No active session to end");
            return;
        }

        Debug.Log("Ending session...");
        isSessionActive = false;

        // Stop the periodic update coroutine
        if (sessionUpdateCoroutine != null)
        {
            StopCoroutine(sessionUpdateCoroutine);
            sessionUpdateCoroutine = null;
        }

        // Update Firebase to mark session as inactive
        StartCoroutine(SetSessionInactive());

        // Trigger event
        OnSessionEnded?.Invoke();
    }

    /// <summary>
    /// Get remaining daily usage time in minutes
    /// </summary>
    /// <param name="callback">Callback with remaining minutes (0 if limit reached)</param>
    public void GetRemainingDailyTime(System.Action<int> callback)
    {
        StartCoroutine(GetRemainingTimeCoroutine(callback));
    }

    /// <summary>
    /// Check if user has reached their daily usage limit
    /// </summary>
    /// <param name="callback">Callback with true if limit reached</param>
    public void CheckDailyLimitReached(System.Action<bool> callback)
    {
        GetRemainingDailyTime((remainingMinutes) =>
        {
            callback?.Invoke(remainingMinutes <= 0);
        });
    }

    /// <summary>
    /// Force end sessions on other devices and continue on current device
    /// Use with caution - should only be called after user confirmation
    /// </summary>
    public void ForceEndOtherSessions()
    {
        Debug.Log("Force ending other sessions...");
        StartCoroutine(ForceEndOtherSessionsCoroutine());
    }

    /// <summary>
    /// Update time budget by adding specified minutes to daily usage
    /// Useful for premium features or time extensions
    /// </summary>
    /// <param name="additionalMinutes">Minutes to add to daily usage limit</param>
    /// <param name="callback">Callback when operation completes</param>
    public void UpdateTimeBudget(int additionalMinutes, System.Action<bool> callback = null)
    {
        StartCoroutine(UpdateTimeBudgetCoroutine(additionalMinutes, callback));
    }

    /// <summary>
    /// Reset daily usage counter (for premium users or new day)
    /// </summary>
    /// <param name="callback">Callback when operation completes</param>
    public void ResetDailyUsage(System.Action<bool> callback = null)
    {
        StartCoroutine(ResetDailyUsageCoroutine(callback));
    }

    #endregion

    #region Private Session Management

    /// <summary>
    /// Check for existing sessions and start new one if allowed
    /// </summary>
    IEnumerator CheckAndStartSession()
    {
        // Wait for userId to be set
        while (string.IsNullOrEmpty(userId))
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return StartCoroutine(CheckExistingSession());
    }

    /// <summary>
    /// Check Firebase for existing session data
    /// Handles device conflicts and usage limits
    /// </summary>
    IEnumerator CheckExistingSession()
    {
        string url = $"{firebaseUrl}/data/users/{userId}.json";
        if (!string.IsNullOrEmpty(firebaseSecret))
            url += $"?auth={firebaseSecret}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log($"Firebase response: {jsonResponse}");

                if (jsonResponse != "null" && !string.IsNullOrEmpty(jsonResponse))
                {
                    UserSession existingSession = JsonConvert.DeserializeObject<UserSession>(jsonResponse);

                    // Check if user is active on another device
                    if (existingSession.isActive && existingSession.deviceId != deviceId)
                    {
                        Debug.LogWarning($"User active on device: {existingSession.deviceName}");
                        ShowDeviceConflictDialog(existingSession.deviceName);
                        OnDeviceConflict?.Invoke();
                        yield break;
                    }

                    // Reset daily usage if it's a new day
                    if (!IsSameDay(existingSession.lastActivity))
                    {
                        Debug.Log("New day detected, resetting daily usage");
                        existingSession.dailyUsageMinutes = 0;
                    }

                    // Check daily usage limit
                    if (existingSession.dailyUsageMinutes >= dailyUsageLimitMinutes)
                    {
                        Debug.LogWarning($"Daily limit reached: {existingSession.dailyUsageMinutes}/{dailyUsageLimitMinutes}");
                        ShowUsageLimitDialog(existingSession.dailyUsageMinutes);
                        OnUsageLimitReached?.Invoke();
                        yield break;
                    }

                    StartNewSession(existingSession.dailyUsageMinutes);
                }
                else
                {
                    // First time user
                    Debug.Log("First time user, starting new session");
                    StartNewSession(0);
                }
            }
            else
            {
                Debug.LogError($"Failed to check session: {request.error}");
                // Continue anyway for offline capability
                StartNewSession(0);
            }
        }
    }

    /// <summary>
    /// Start a new session with given daily usage
    /// </summary>
    /// <param name="previousDailyUsage">Previous daily usage in minutes</param>
    void StartNewSession(int previousDailyUsage)
    {
        sessionStart = DateTime.Now;
        isSessionActive = true;
        currentSessionMinutes = 0;

        UserSession newSession = new UserSession
        {
            userId = userId,
            deviceId = deviceId,
            sessionStart = DateTimeToUnixTimestamp(sessionStart),
            dailyUsageMinutes = previousDailyUsage,
            lastActivity = DateTimeToUnixTimestamp(DateTime.Now),
            isActive = true,
            deviceName = SystemInfo.deviceName
        };

        // Update Firebase and start periodic updates
        StartCoroutine(UpdateSessionInFirebase(newSession));
        sessionUpdateCoroutine = StartCoroutine(UpdateSessionPeriodically());

        Debug.Log($"Session started successfully. Previous daily usage: {previousDailyUsage} minutes");
        OnSessionStarted?.Invoke();
    }

    /// <summary>
    /// Periodically update session data in Firebase
    /// Adds time budget and checks for usage limits
    /// </summary>
    IEnumerator UpdateSessionPeriodically()
    {
        while (isSessionActive)
        {
            yield return new WaitForSeconds(sessionUpdateIntervalSeconds);

            currentSessionMinutes = (int)(DateTime.Now - sessionStart).TotalMinutes;

            // Get current data from Firebase and update it
            yield return StartCoroutine(GetCurrentSessionData((currentData) =>
            {
                if (currentData != null)
                {
                    // Add time budget (configurable minutes per update)
                    currentData.dailyUsageMinutes += timeBudgetUpdateMinutes;
                    currentData.lastActivity = DateTimeToUnixTimestamp(DateTime.Now);

                    Debug.Log($"Updated usage: {currentData.dailyUsageMinutes}/{dailyUsageLimitMinutes} minutes");

                    // Check if daily limit reached
                    if (currentData.dailyUsageMinutes >= dailyUsageLimitMinutes)
                    {
                        Debug.LogWarning("Daily usage limit reached during session");
                        ShowUsageLimitDialog(currentData.dailyUsageMinutes);
                        OnUsageLimitReached?.Invoke();
                        EndSession();
                        return;
                    }

                    // Update Firebase with new data
                    StartCoroutine(UpdateSessionInFirebase(currentData));
                }
            }));
        }
    }

    #endregion

    #region Firebase Operations

    /// <summary>
    /// Get current session data from Firebase
    /// </summary>
    /// <param name="callback">Callback with session data or null if not found</param>
    IEnumerator GetCurrentSessionData(System.Action<UserSession> callback)
    {
        string url = $"{firebaseUrl}/users/{userId}.json";
        if (!string.IsNullOrEmpty(firebaseSecret))
            url += $"?auth={firebaseSecret}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                if (jsonResponse != "null" && !string.IsNullOrEmpty(jsonResponse))
                {
                    UserSession session = JsonConvert.DeserializeObject<UserSession>(jsonResponse);
                    callback?.Invoke(session);
                }
                else
                {
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"Failed to get current session data: {request.error}");
                callback?.Invoke(null);
            }
        }
    }

    /// <summary>
    /// Update session data in Firebase
    /// </summary>
    /// <param name="session">Session data to update</param>
    IEnumerator UpdateSessionInFirebase(UserSession session)
    {
        string url = $"{firebaseUrl}/users/{userId}.json";
        if (!string.IsNullOrEmpty(firebaseSecret))
            url += $"?auth={firebaseSecret}";

        string jsonData = JsonConvert.SerializeObject(session);

        using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to update session: {request.error}");
            }
            else
            {
                Debug.Log("Session updated successfully in Firebase");
            }
        }
    }

    /// <summary>
    /// Mark current session as inactive in Firebase
    /// </summary>
    IEnumerator SetSessionInactive()
    {
        yield return StartCoroutine(GetCurrentSessionData((currentData) =>
        {
            if (currentData != null)
            {
                currentData.isActive = false;
                currentData.lastActivity = DateTimeToUnixTimestamp(DateTime.Now);
                StartCoroutine(UpdateSessionInFirebase(currentData));
                Debug.Log("Session marked as inactive in Firebase");
            }
        }));
    }

    #endregion

    #region Public Method Coroutines

    /// <summary>
    /// Coroutine to get remaining daily time
    /// </summary>
    IEnumerator GetRemainingTimeCoroutine(System.Action<int> callback)
    {
        yield return StartCoroutine(GetCurrentSessionData((currentData) =>
        {
            if (currentData != null)
            {
                int remaining = dailyUsageLimitMinutes - currentData.dailyUsageMinutes;
                callback?.Invoke(Mathf.Max(0, remaining));
            }
            else
            {
                // If no data found, assume full time available
                callback?.Invoke(dailyUsageLimitMinutes);
            }
        }));
    }

    /// <summary>
    /// Coroutine to force end other sessions
    /// </summary>
    IEnumerator ForceEndOtherSessionsCoroutine()
    {
        yield return StartCoroutine(GetCurrentSessionData((currentData) =>
        {
            if (currentData != null)
            {
                // Take over the session
                currentData.deviceId = deviceId;
                currentData.deviceName = SystemInfo.deviceName;
                currentData.isActive = true;
                currentData.lastActivity = DateTimeToUnixTimestamp(DateTime.Now);
                StartCoroutine(UpdateSessionInFirebase(currentData));

                // Start session if not already active
                if (!isSessionActive)
                {
                    StartNewSession(currentData.dailyUsageMinutes);
                }

                Debug.Log("Successfully took over session from other device");
            }
        }));
    }

    /// <summary>
    /// Coroutine to update time budget
    /// </summary>
    IEnumerator UpdateTimeBudgetCoroutine(int additionalMinutes, System.Action<bool> callback)
    {
        bool success = false;

        yield return StartCoroutine(GetCurrentSessionData((currentData) =>
        {
            if (currentData != null)
            {
                // Add additional minutes to daily usage limit (effectively extending time)
                dailyUsageLimitMinutes += additionalMinutes;

                // Update the session data
                currentData.lastActivity = DateTimeToUnixTimestamp(DateTime.Now);
                StartCoroutine(UpdateSessionInFirebase(currentData));

                success = true;
                Debug.Log($"Time budget updated: +{additionalMinutes} minutes. New limit: {dailyUsageLimitMinutes}");
            }
        }));

        callback?.Invoke(success);
    }

    /// <summary>
    /// Coroutine to reset daily usage
    /// </summary>
    IEnumerator ResetDailyUsageCoroutine(System.Action<bool> callback)
    {
        bool success = false;

        yield return StartCoroutine(GetCurrentSessionData((currentData) =>
        {
            if (currentData != null)
            {
                currentData.dailyUsageMinutes = 0;
                currentData.lastActivity = DateTimeToUnixTimestamp(DateTime.Now);
                StartCoroutine(UpdateSessionInFirebase(currentData));

                success = true;
                Debug.Log("Daily usage reset to 0 minutes");
            }
        }));

        callback?.Invoke(success);
    }

    #endregion

    #region UI Dialog Methods

    /// <summary>
    /// Show device conflict dialog
    /// Override this method to implement custom UI
    /// </summary>
    /// <param name="activeDeviceName">Name of device where user is currently active</param>
    protected virtual void ShowDeviceConflictDialog(string activeDeviceName)
    {
        Debug.LogWarning($"User is active on another device: {activeDeviceName}");
        // TODO: Implement UI dialog with options:
        // - "Switch to this device" (calls ForceEndOtherSessions)
        // - "Continue on other device" (closes app)
    }

    /// <summary>
    /// Show usage limit dialog
    /// Override this method to implement custom UI
    /// </summary>
    /// <param name="minutesUsed">Total minutes used today</param>
    protected virtual void ShowUsageLimitDialog(int minutesUsed)
    {
        Debug.LogWarning($"Daily usage limit reached: {minutesUsed}/{dailyUsageLimitMinutes} minutes");
        // TODO: Implement UI dialog with options:
        // - Show remaining time
        // - Offer premium upgrade
        // - "Try again tomorrow"
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Check if given timestamp is from the same day as today
    /// </summary>
    bool IsSameDay(long unixTimestamp)
    {
        DateTime date = UnixTimestampToDateTime(unixTimestamp);
        return date.Date == DateTime.Now.Date;
    }

    /// <summary>
    /// Convert DateTime to Unix timestamp
    /// </summary>
    long DateTimeToUnixTimestamp(DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Convert Unix timestamp to DateTime
    /// </summary>
    DateTime UnixTimestampToDateTime(long unixTimestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
    }

    #endregion
}