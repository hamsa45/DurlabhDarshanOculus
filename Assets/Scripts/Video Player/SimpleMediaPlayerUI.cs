using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

public class SimpleMediaPlayerUI : MonoBehaviour
{
    public CanvasGroup videoPlayerControls;
    public MediaPlayer mediaPlayer;
    public VolumeControlVideoPlayer volumeController;
    public SettingVideoPlayer settingsController;

    [Header("UI")]

    public TextMeshProUGUI videoTitleText;    
    public TextMeshProUGUI CurrentTimeText;
    public TextMeshProUGUI DurationText;
    public TwoStateButton playPauseButton;

    public Button skipBackwardButton;
    public Button skipForwardButton;
    public Button exitButton;
    public Slider timeSlider;
    public TextMeshProUGUI messageText;
    public GameObject bufferingIndicator;

    [Header("Options")]
    public float skipSeconds = 5f;

    private bool isDraggingSlider = false;
    private SliderDragHandler sliderDragHandler;

    [SerializeField] private float fadeInOutTime = 0.5f;
    
    private bool ifFirstFrameReady = false;

    void HandleEvent(MediaPlayer mp, MediaPlayerEvent.EventType eventType, ErrorCode code)
    {
        if(eventType == MediaPlayerEvent.EventType.FirstFrameReady)
        {
            ifFirstFrameReady = true;
        }
        else if (eventType == MediaPlayerEvent.EventType.Error)
        {
            Debug.LogError("AvPro Error: " + code);
            ShowMessage("Error");
        }
        else if (eventType == MediaPlayerEvent.EventType.FinishedPlaying)
        {
            ShowMessage("Finished Playing");
        }
        else if (eventType == MediaPlayerEvent.EventType.Stalled)
        {
            ShowMessage("Stalled");
            bufferingIndicator.SetActive(true);
        }
        else if (eventType == MediaPlayerEvent.EventType.Unstalled)
        {
            ShowMessage("Unstalled");
            bufferingIndicator.SetActive(false);
        }
    }
    void Start()
    {
        if (!mediaPlayer) return;
        mediaPlayer.Events.AddListener(HandleEvent);
        playPauseButton.GetComponent<Button>().onClick.AddListener(TogglePlayPause);
        skipForwardButton.onClick.AddListener(() => SeekRelative(skipSeconds));
        skipBackwardButton.onClick.AddListener(() => SeekRelative(-skipSeconds));
        exitButton.onClick.AddListener(LoadHomeScene);
        
        // Setup slider drag handler
        SetupSliderDragHandler();
        
        // Register slider value change listener
        timeSlider.onValueChanged.AddListener(OnSliderChanged);
        
        bufferingIndicator.SetActive(false);
        messageText.text = "";
        
        // Set initial play/pause button state
        UpdatePlayPauseButtonState();
    }

    public void SetVideoTitle(string title)
    {
        videoTitleText.text = title;
    }

    void SetupSliderDragHandler()
    {
        // Add the SliderDragHandler component if it doesn't exist
        sliderDragHandler = timeSlider.gameObject.GetComponent<SliderDragHandler>();
        if (sliderDragHandler == null)
        {
            sliderDragHandler = timeSlider.gameObject.AddComponent<SliderDragHandler>();
        }
        
        // Configure the handler
        sliderDragHandler.slider = timeSlider;
        sliderDragHandler.mediaPlayer = mediaPlayer;
        
        // Register events
        sliderDragHandler.OnDragBegin += () => isDraggingSlider = true;
        sliderDragHandler.OnDragEnd += (value) => isDraggingSlider = false;
    }

    void UpdateTimeText()
    {
        CurrentTimeText.text = TimeFormat(mediaPlayer.Control.GetCurrentTime());
        DurationText.text = TimeFormat(mediaPlayer.Info.GetDuration());
    }

    string TimeFormat(double time)
    {
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }

    void Update()   
    {
        if (!mediaPlayer || mediaPlayer.Control == null || mediaPlayer.Info == null || !ifFirstFrameReady) return;

        if(OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.I))
        {
            FadeInOutVideoPlayerControls(); 
        }

        // Time slider
        if (!isDraggingSlider && mediaPlayer.Info.GetDuration() > 0)
        {
            float time = (float)(mediaPlayer.Control.GetCurrentTime() / mediaPlayer.Info.GetDuration());
            timeSlider.value = Mathf.Clamp01(time);
        }
        
        // Update play/pause button visuals based on media player state
        UpdatePlayPauseButtonState();
    }

    void FixedUpdate()
    {
        UpdateTimeText();
    }

    void UpdateSlider()
    {
        timeSlider.maxValue = (float)mediaPlayer.Info.GetDuration();
        timeSlider.minValue = 0;
    }

    void FadeInOutVideoPlayerControls()
    {
        if(videoPlayerControls.alpha == 1)
        {
            UIFadeManager.Instance.FadeOut(videoPlayerControls, fadeInOutTime);
        }
        else
        {
            UIFadeManager.Instance.FadeIn(videoPlayerControls, fadeInOutTime);
        }
    }

    void UpdatePlayPauseButtonState()
    {
        if (mediaPlayer.Control != null)
        {
            // Use StateOne for Pause (meaning video is playing)
            // Use StateTwo for Play (meaning video is paused)
            TwoStateButton.State newState = mediaPlayer.Control.IsPlaying() 
                ? TwoStateButton.State.StateOne    // Show pause icon when playing
                : TwoStateButton.State.StateTwo;   // Show play icon when paused
                
            // Only update if state has changed
            if (playPauseButton.CurrentState != newState)
            {
                playPauseButton.SetState(newState);
            }
        }
    }

    void TogglePlayPause()
    {
        if (mediaPlayer.Control.IsPlaying())
        {
            mediaPlayer.Pause();
            ShowMessage("Paused");
        }
        else
        {
            mediaPlayer.Play();
            ShowMessage("Playing");
        }
        
        // Update the button state immediately
        UpdatePlayPauseButtonState();
    }

    void SeekRelative(float seconds)
    {
        double newTime = mediaPlayer.Control.GetCurrentTime() + seconds;
        newTime = Mathf.Clamp((float)newTime, 0, (float)mediaPlayer.Info.GetDuration());
        mediaPlayer.Control.Seek(newTime);
        ShowMessage(seconds > 0 ? "Skipped Forward" : "Skipped Backward");
    }

    void OnSliderChanged(float value)
    {
        if (!isDraggingSlider || mediaPlayer.Info.GetDuration() <= 0) return;
        
        // Update time text while dragging but don't seek yet
        CurrentTimeText.text = TimeFormat(value * mediaPlayer.Info.GetDuration());
    }

    void ShowMessage(string text)
    {
        messageText.text = text;
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), 2f);
    }

    void ClearMessage()
    {
        messageText.text = "";
    }

    /// <summary>
    /// Call this method when the current video changes to update the quality options
    /// </summary>
    public void UpdateQualityOptions()
    {
        // Forward to settings controller if available
        if (settingsController != null)
        {
            settingsController.UpdateQualityOptions();
        }
    }

    private void LoadHomeScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
    }
}
