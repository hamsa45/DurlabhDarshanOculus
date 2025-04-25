using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class URLHelper : MonoBehaviour
{
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void OpenURL(string url);
#endif

    public static void OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogError("Invalid URL.");
            return;
        }

        try
        {
#if UNITY_IOS
            OpenIOSURL(url);
#elif UNITY_ANDROID
            OpenAndroidURL(url);
#else
            Application.OpenURL(url);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to open URL: {url}, Error: {ex.Message}");
        }
    }

#if UNITY_IOS
    private static void OpenIOSURL(string url)
    {
        OpenURL(url);
    }
#endif

#if UNITY_ANDROID
    private static void OpenAndroidURL(string url)
    {
        Application.OpenURL(url);
    }
#endif
}
