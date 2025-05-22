/*using Oculus.Platform;
using System;
using System.Collections;
using UnityEngine.Networking;

public class UsageTracker : MonoBehaviour
{
    private string userId;
    private DateTime sessionStart;
    private int dailyUsageLimit = 120; // 2 hours in minutes

    void Start()
    {
        GetUserID();
        CheckExistingSession();
    }

    void CheckExistingSession()
    {
        // API call to check if user is active on another device
        StartCoroutine(CheckActiveSession(userId));
    }

    IEnumerator CheckActiveSession(string userId)
    {
        string url = $"https://yourapi.com/check-session/{userId}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonUtility.FromJson<SessionResponse>(request.downloadHandler.text);

                if (response.isActiveElsewhere)
                {
                    ShowDeviceConflictDialog();
                    yield return null;
                }

                if (response.dailyUsageMinutes >= dailyUsageLimit)
                {
                    ShowUsageLimitDialog();
                    yield return null;
                }

                StartNewSession();
            }
        }
    }

    void StartNewSession()
    {
        sessionStart = DateTime.Now;
        StartCoroutine(UpdateSessionPeriodically());
    }

    IEnumerator UpdateSessionPeriodically()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f); // Update every minute

            int sessionMinutes = (int)(DateTime.Now - sessionStart).TotalMinutes;
            UpdateUsageOnServer(userId, sessionMinutes);

            // Check if daily limit reached
            if (GetTotalDailyUsage() >= dailyUsageLimit)
            {
                ShowUsageLimitDialog();
                yield break;
            }
        }
    }

    void GetUserID()
    {
        Users.GetLoggedInUser().OnComplete(message =>
        {
            if (!message.IsError)
            {
                var user = message.Data;
                string userId = user.ID.ToString();
                string username = user.OculusID;

                // Store this ID for your database tracking
                StoreUserInDatabase(userId, username);
            }
        });
    }
}*/