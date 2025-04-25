using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class NetworkMonitor : MonoBehaviour
{
    
    [Header("Controls")]
    [Tooltip("Enable to print debug messages in console. Disable for production builds to improve performance.")]
    [SerializeField] private bool printDebugMessages;

    [Tooltip("Start checking connectivity on awake.")]
    [SerializeField] private bool StartOnAwake = true;


    [Header("Ping parameters")]
    [Tooltip("Ping this url to test connection. Recommendation: Use link to your own website page with simple text.")]
    [SerializeField]
    private string[] pingUrl =
        new string[3] { "https://google.com", "https://www.amazon.com/", "https://www.cloudflare.com/" };

    [Tooltip("Set how often is connection tested in seconds.")]
    public float pingInterval = 5f;

    //[Header("Events")]
    //[SerializeField] private UnityConnectivityEvent OnConnectivityChange = new UnityConnectivityEvent();

    public static NetworkMonitor Instance;

    private UnityWebRequest webRequest;
    private bool isConnected;
    private Coroutine connectionTestCoroutine;
    private bool isConnectedDebugToggle = true;

    public static UnityEvent<bool> OnInternetStatusChanged = new();
    /// <summary>
    /// Use this property to check if Connectivity Checker is running
    /// or it was stopped.
    /// </summary>
    public bool IsTestingConnectivity { get; private set; } = false;

    /// <summary>
    /// This property works only in editor and is used to toggle fake connect / disconnect
    /// </summary>
    public bool IsConnectedDebugToggle
    {
        get
        {
            return isConnectedDebugToggle;
        }
        set
        {
            if (Application.isEditor)
            {
                isConnectedDebugToggle = value;
            }
            else
            {
                isConnectedDebugToggle = true;
            }
        }
    }

    /// <summary>
    /// This property should be used to check if there is internet connectivity or not.
    /// If there is internet connection this property will return true, otherwise false.
    /// </summary>
    public bool IsConnected
    {
        get
        {
            return isConnected;
        }
        private set
        {
            if (isConnected == value) return;
            isConnected = value;
            //OnConnectivityChange.Invoke(isConnected, webRequest.error);
            PrintDebugMessage($"Is Connected:: {isConnected}", MessageType.Verbose);
            Debug.LogWarning($"Is Connected:: {isConnected}");
            OnInternetStatusChanged?.Invoke(isConnected);
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance.gameObject);

            if (StartOnAwake) StartConnectionCheck();
        }
    }

    /// <summary>
    /// Start testing connectivity. Use "StartOnAwake" bool to start checking connectivity
    /// on awake. Or disable "StartOnAwake" and call this method when you need to start checking.
    /// </summary>
    public void StartConnectionCheck()
    {
        if (connectionTestCoroutine == null)
        {
            connectionTestCoroutine = StartCoroutine(TestConnection());
            IsTestingConnectivity = true;
        }
        else
        {
            PrintDebugMessage("Connection check already started!", MessageType.Warning);
        }
    }

    /// <summary>
    /// Use this method to stop connectivity check when this check is no more needed.
    /// If not stopped the check is continuous and will run every ping interval
    /// (according to settings in ConnectivityManager inspector) and will not stop by itself.
    /// </summary>
    public void StopConnectionCheck()
    {
        if (connectionTestCoroutine != null)
        {
            StopCoroutine(connectionTestCoroutine);
            connectionTestCoroutine = null;
            IsTestingConnectivity = false;
        }
        else
        {
            PrintDebugMessage("No active Connection check!", MessageType.Warning);
        }
    }

    public void CheckInternetConntection(Action<bool> connected)
	{
        StartCoroutine(immediateInternetConnectionCheck(connected));
	}

    private IEnumerator immediateInternetConnectionCheck(Action<bool> connected)
    {
        string url = pingUrl[UnityEngine.Random.Range(0, pingUrl.Length - 1)];

        using (UnityWebRequest request = UnityWebRequest.Head(url))
        {
            request.timeout = 3;
            yield return request.SendWebRequest();

            bool isConnected = request.result == UnityWebRequest.Result.Success;
            connected?.Invoke(isConnected);
        }
    }


    private IEnumerator TestConnection()
    {
        while (true)
        {
            string url = pingUrl[UnityEngine.Random.Range(0, pingUrl.Length - 1)];

            if (!string.IsNullOrEmpty(url))
            {
                webRequest = new UnityWebRequest(url);
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    if (Application.isEditor)
                    {
                        IsConnected = IsConnectedDebugToggle;
                    }
                    else
                    {
                        IsConnected = true;
                    }
                }
                else if (webRequest.result != UnityWebRequest.Result.InProgress)
                {
                    IsConnected = false;
                    PrintDebugMessage($"Connection Error::{webRequest.error}", MessageType.Warning);
                }
            }
            else
            {
                IsConnected = false;
                PrintDebugMessage($"Ping URL in Connectivity Manager ( Inspector ) is missing", MessageType.Error);
            }
            yield return new WaitForSeconds(pingInterval);
        }
    }

    private void PrintDebugMessage(string msg, MessageType msgType)
    {
        if (printDebugMessages)
        {
            switch (msgType)
            {
                case MessageType.Warning:
                    Logs.LogWarning($"ConnectivityManager:: {msg}");
                    break;
                case MessageType.Error:
                    Logs.LogError($"ConnectivityManager:: {msg}");
                    break;
                default:
                    Logs.Log($"ConnectivityManager:: {msg}");
                    break;
            }
        }
    }

    private enum MessageType
    {
        Verbose,
        Warning,
        Error
    }
}
