using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;
using TMPro;
using System;

public class VolumeControlVideoPlayer : MonoBehaviour, IAutoHideUI
{
    private class VolumeIcons{
        public Sprite sprite;
        public float volume;
    }

    [Header("References")]
    public MediaPlayer mediaPlayer;
    public Button volumeButton;
    public Slider volumeSlider;
    public TextMeshProUGUI messageText;

    [Header("Options")]
    public float defaultVolume = 1.0f;

    private CanvasGroup volumeSliderCanvasGroup;
    private bool isVolumeSliderVisible = false;
    private float maxSliderValue = 10;
    private AutoHideUIComponent autoHideComponent;

    // Start is called before the first frame update
    void Start()
    {
        // Check if we have all the required references
        if (mediaPlayer == null)
        {
            Debug.LogError("MediaPlayer reference is missing on VolumeControlVideoPlayer");
            enabled = false;
            return;
        }

        // Setup volume button if available
        if (volumeButton != null)
        {
            volumeButton.onClick.AddListener(ToggleVolumeSlider);
        }

        // Setup volume slider
        if (volumeSlider != null)
        {
            // Setup canvas group for fading
            volumeSliderCanvasGroup = volumeSlider.GetComponent<CanvasGroup>();
            if (volumeSliderCanvasGroup == null)
            {
                volumeSliderCanvasGroup = volumeSlider.gameObject.AddComponent<CanvasGroup>();
            }
            volumeSliderCanvasGroup.alpha = 0;
            volumeSliderCanvasGroup.interactable = false;
            volumeSliderCanvasGroup.blocksRaycasts = false;
            isVolumeSliderVisible = false;

            // Set initial value and add listener
            volumeSlider.value = mediaPlayer.AudioVolume;
            maxSliderValue = volumeSlider.maxValue;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
        else
        {
            // If no slider, just set the default volume
            mediaPlayer.AudioVolume = defaultVolume;
        }

        // Add AutoHideUIComponent
        autoHideComponent = gameObject.AddComponent<AutoHideUIComponent>();
    }

    /// <summary>
    /// Toggles the visibility of the volume slider
    /// </summary>
    public void ToggleVolumeSlider()
    {
        if (isVolumeSliderVisible)
        {
            autoHideComponent.Hide();
        }
        else
        {
            autoHideComponent.Show();
        }
    }
    
    /// <summary>
    /// Sets the volume of the media player
    /// </summary>
    public void SetVolume(float volume)
    {
        if (mediaPlayer != null)
        {
            mediaPlayer.AudioVolume = volume/maxSliderValue;
            
            if (messageText != null)
            {
                ShowMessage($"Volume: {(int)(volume * 10)}%");
            }
            
            autoHideComponent.OnInteract();
        }
    }

    // IAutoHideUI Implementation
    public CanvasGroup GetCanvasGroup()
    {
        return volumeSliderCanvasGroup;
    }

    public void OnShow()
    {
        isVolumeSliderVisible = true;
        if (messageText != null)
        {
            ShowMessage("Volume Control");
        }
    }

    public void OnHide()
    {
        isVolumeSliderVisible = false;
    }

    public void OnInteract()
    {
        // Additional interaction handling if needed
    }

    /// <summary>
    /// Displays a message to the user
    /// </summary>
    private void ShowMessage(string text)
    {
        if (messageText == null)
            return;
            
        messageText.text = text;
        CancelInvoke(nameof(ClearMessage));
        Invoke(nameof(ClearMessage), 2f);
    }

    /// <summary>
    /// Clears the message text
    /// </summary>
    private void ClearMessage()
    {
        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    /// <summary>
    /// Mutes or unmutes the audio
    /// </summary>
    public void ToggleMute()
    {
        if (mediaPlayer == null || volumeSlider == null)
            return;
            
        if (mediaPlayer.AudioVolume > 0)
        {
            // Store current volume before muting
            PlayerPrefs.SetFloat(PlayerPrefsConst.VOLUME, mediaPlayer.AudioVolume);

            SetVolume(0);
            
            if (messageText != null)
            {
                ShowMessage("Muted");
            }
        }
        else
        {
            // Restore previous volume
            float previousVolume = 1f;
            if (PlayerPrefs.HasKey(PlayerPrefsConst.VOLUME))
            {
                previousVolume = PlayerPrefs.GetFloat(PlayerPrefsConst.VOLUME);
            }
            
            SetVolume(previousVolume);
            volumeSlider.value = previousVolume;
            
            if (messageText != null)
            {
                ShowMessage("Unmuted");
            }
        }
    }
}

