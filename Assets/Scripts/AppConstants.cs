using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppConstants : MonoBehaviour
{
    public static readonly string m3u8CacheFolder = Path.Combine(GetInternalDataPath(), ".hidden", "VideoCache");

    public static readonly int m3u8CacheExpirationDayLimit = 15;

    public static string VideoThumbnailJsonUrl { get; internal set; } = "https://d2ak1mtdwv7inq.cloudfront.net/DurlabhDarshan/Jsons/VideoThumbnails.json";

    public static string GetInternalDataPath()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject filesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir");
            string path = filesDir.Call<string>("getAbsolutePath");
            return path;
        }
#else
        return Application.persistentDataPath;
#endif
    }

     public static readonly List<string> disactiveVideoShowIds = new List<string>
    {
         "3", "11"
    };
}

public static class Logs
{
    public static bool IsDebugEnabled = true;
    public static void Log(string message)
    {
        if (IsDebugEnabled)
        {
            Debug.Log($"[DEBUG] {message}");
        }
    }

    public static void LogWarning(string message)
    {
        if (IsDebugEnabled)
        {
            Debug.LogWarning($"[DEBUG] {message}");
        }
    }

    public static void LogError(string message)
    {
        if (IsDebugEnabled)
        {
            Debug.LogError($"[DEBUG] {message}");
        }
    }

    public static void LogException(System.Exception exception)
    {
        if (IsDebugEnabled)
        {
            Debug.LogException(exception);
        }
    }
}

public static class Scenes
{
     public const string Home = "Home_UI";
    public const string Login = "Login";
    public const string QRActivation = "QRActivation";
}

public static class SceneLoader
{
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    
}
