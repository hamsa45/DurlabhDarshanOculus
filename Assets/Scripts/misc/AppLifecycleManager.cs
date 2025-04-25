using UnityEngine;

public class AppLifecycleManager : MonoBehaviour
{
    private bool isQuitting = false;

    private void OnEnable()
    {
        // Subscribe to the application lifecycle events
        Application.quitting += OnQuitting;
    }

    private void OnDisable()
    {
        // Unsubscribe from the application lifecycle events
        Application.quitting -= OnQuitting;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // The application is going into the background
            Debug.Log("Application paused");
        }
        else
        {
            // The application is coming back from the background
            Debug.Log("Application resumed");
        }
    }

    private void OnApplicationQuit()
    {
        // The application is quitting
        isQuitting = true;
        Debug.Log("Application quitting");
    }

    private void OnQuitting()
    {
        // This method is called when the application is about to quit
        // This is different from OnApplicationQuit and may be useful for cleanup
        Debug.Log("Quitting event triggered");
    }

    private void Update()
    {
        // Example: Check if the application was quit from the recent apps menu
        if (isQuitting)
        {
            Debug.Log("Application quit from recent apps menu");
            // Add your cleanup code or additional logic here
        }
    }
}

