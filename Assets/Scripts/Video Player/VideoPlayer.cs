using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class VideoPlayer : MonoBehaviour
{
    public static VideoPlayer instance;
    public MediaPlayer mediaPlayer;

    public GameObject VideoBufferingLoader;
    public GameObject InternetPanel;
    public GameObject MenuPanel;
    private Vector3 menuInitialPosition;
    private Vector3 menuTargetPosition;
    private float menuLerpSpeed = 2f;
    private bool isMenuLerping = false;


    private bool isStalled;

    private void Awake()
    {
        instance = this;
        PlayVideo();
    }

    // Play video from the selected video panel
    private void PlayVideo()
    {
        mediaPlayer.Play();
        mediaPlayer.Events.AddListener(HandleEvent);
    }



    private void FixedUpdate()
    {
        Screen.brightness = 1.0f;
        if (!isStalled)
        {
            if (VideoBufferingLoader.activeSelf)
                VideoBufferingLoader.SetActive(false);
        }
        else
        {
            if (!VideoBufferingLoader.activeSelf)
                VideoBufferingLoader.SetActive(true);
        }
    }

    IEnumerator CheckInternetConnection(Action<bool> syncResult)
    {
        const string echoServer = "https://google.com";

        bool result = false;
        while (!result)
        {
            using (var request = UnityWebRequest.Head(echoServer))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();
                result = request.result == UnityWebRequest.Result.Success;
                Debug.Log("internet is connected : " + result);
                yield return new WaitForSeconds(2);
            }
            syncResult(result);
        }
        //syncResult(result);
        if (result)
            yield break;
    }

    void HandleEvent(MediaPlayer mp, MediaPlayerEvent.EventType eventType, ErrorCode code)
    {
        if (eventType == MediaPlayerEvent.EventType.FirstFrameReady) {
            VideoBufferingLoader.SetActive(false);
        }
        else if (eventType == MediaPlayerEvent.EventType.Error)
        {
            CheckInternetConnection(c =>
            {
                InternetPanel.SetActive(c);
            });
        }
        else if (eventType == MediaPlayerEvent.EventType.Stalled)
        {
            isStalled = true;
        }
        else if (eventType == MediaPlayerEvent.EventType.Unstalled)
        {
            isStalled = false;
        }
    }

    private void Update()
    {
        bool input = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        Debug.Log("input : " + input);
        if (input)
        {
            MenuPanel.SetActive(!MenuPanel.activeSelf);
        }

        if (MenuPanel.activeSelf)   
            LerpMenuPosition(); 
    }


    public void ChangeScene()
    {

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            SceneManager.LoadScene(0);
        }
    }

    private void LerpMenuPosition(){
        if (isMenuLerping)
        {
            MenuPanel.transform.position = Vector3.Lerp(menuInitialPosition, menuTargetPosition, menuLerpSpeed * Time.deltaTime);
            if (Vector3.Distance(MenuPanel.transform.position, menuTargetPosition) < 0.01f)
            {
                MenuPanel.transform.position = menuTargetPosition;
                isMenuLerping = false;
            }
        }
    }

    public void PlayVideo(string url)
    {
        // Implement your video playback logic here
        Debug.Log($"Playing video from URL: {url}");
    }
}
