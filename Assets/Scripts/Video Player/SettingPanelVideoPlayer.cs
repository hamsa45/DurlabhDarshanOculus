using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProVideo;
using TMPro;
using System;

public class SettingVideoPlayer : MonoBehaviour, IAutoHideUI
{

    [Header("References")]
    public MediaPlayer mediaPlayer;
    public Button settingsButton;
    public GameObject settingsPanel;
    public TMP_Dropdown qualityDropdown;
    public TextMeshProUGUI messageText;

    [Header("Options")]
    public float fadeDuration = 0.3f;
    
    private CanvasGroup settingsPanelCanvasGroup;
    private bool isSettingsPanelVisible = false;
    private AutoHideUIComponent autoHideComponent;

    void OnEnable()
    {
        if (autoHideComponent == null)
        {
            autoHideComponent = gameObject.AddComponent<AutoHideUIComponent>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            ToggleSettingsPanel();
        }
    }

    void Start()
    {
        if (mediaPlayer == null)
        {
            Debug.LogError("MediaPlayer reference is missing on SettingVideoPlayer");
            enabled = false;
            return;
        }

        // Setup settings button if available
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(ToggleSettingsPanel);
        }

        // Setup settings panel if available
        if (settingsPanel != null)
        {
            // Setup canvas group for fading
            settingsPanelCanvasGroup = settingsPanel.GetComponent<CanvasGroup>();
            if (settingsPanelCanvasGroup == null)
            {
                settingsPanelCanvasGroup = settingsPanel.AddComponent<CanvasGroup>();
            }
            
            // Initialize panel as hidden
            settingsPanelCanvasGroup.alpha = 0;
            settingsPanelCanvasGroup.interactable = false;
            settingsPanelCanvasGroup.blocksRaycasts = false;
            isSettingsPanelVisible = false;
        }

        // Set up quality dropdown if available
        if (qualityDropdown != null)
        {
            SetupQualityDropdown();
            qualityDropdown.onValueChanged.AddListener(ChangeQuality);
        }

        qualityDropdown.value = PlayerPrefs.GetInt(PlayerPrefsConst.QUALITY, 0);
        ChangeQuality(PlayerPrefs.GetInt(PlayerPrefsConst.QUALITY));
    }

    /// <summary>
    /// Toggles the visibility of the settings panel
    /// </summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel == null || settingsPanelCanvasGroup == null)
            return;
            
        if (isSettingsPanelVisible)
        {
            autoHideComponent.Hide();
        }
        else
        {
            autoHideComponent.Show();
        }
    }
    
    /// <summary>
    /// Changes the video quality based on the dropdown selection
    /// </summary>
    public void ChangeQuality(int index)
    {
        if (VideoPlayer.instance.videoQualityOptions == null || qualityDropdown == null || qualityDropdown.options.Count <= index)
            return;
            
        string newUrl = null;
        string selectedOption = qualityDropdown.options[index].text;
        
        // Save current timestamp before changing quality
        double currentTime = mediaPlayer.Control.GetCurrentTime();
        
        switch (selectedOption)
        {
            case "6K":
                if (VideoPlayer.instance.videoQualityOptions.Quality6K != null)
                    newUrl = VideoPlayer.instance.videoQualityOptions.Quality6K.Url;
                break;
            case "4K":
                if (VideoPlayer.instance.videoQualityOptions.Quality4K != null)
                    newUrl = VideoPlayer.instance.videoQualityOptions.Quality4K.Url;
                break;
            case "2K":
                if (VideoPlayer.instance.videoQualityOptions.Quality2K != null)
                    newUrl = VideoPlayer.instance.videoQualityOptions.Quality2K.Url;
                break;
            case "Adaptive":
                if (VideoPlayer.instance.videoQualityOptions.AdaptiveStream != null)
                    newUrl = VideoPlayer.instance.videoQualityOptions.AdaptiveStream.Url;
                break;
            default:
                break;
        }
        
        if (!string.IsNullOrEmpty(newUrl))
        {
            Debug.Log($"Switching to {newUrl}");
            mediaPlayer.OpenMedia(MediaPathType.AbsolutePathOrURL, newUrl, true);
            
            // Wait for the video to be ready before seeking to the saved timestamp
            StartCoroutine(SeekToTimeAfterLoad(currentTime));
            
            ShowMessage("Switched to " + selectedOption + " Quality");
            PlayerPrefs.SetInt(PlayerPrefsConst.QUALITY, index);
            autoHideComponent.OnInteract();
        }
    }

    private IEnumerator SeekToTimeAfterLoad(double timeInSeconds)
    {
        // Wait until the video is ready to play
        while (!mediaPlayer.Control.IsPlaying())
        {
            yield return null;
        }
        
        // Seek to the saved timestamp
        mediaPlayer.Control.Seek(timeInSeconds);
    }

    /// <summary>
    /// Sets up the quality dropdown options based on the available video qualities
    /// </summary>
    public void SetupQualityDropdown()
    {
        if (qualityDropdown == null)
            return;
            
        qualityDropdown.ClearOptions();
        
        List<string> options = new List<string>();
        
        if (VideoPlayer.instance.videoQualityOptions != null)
        {
                
            if (VideoPlayer.instance.videoQualityOptions.Quality6K != null && !string.IsNullOrEmpty(VideoPlayer.instance.videoQualityOptions.Quality6K.Url))
                options.Add("6K");
                
            if (VideoPlayer.instance.videoQualityOptions.Quality4K != null && !string.IsNullOrEmpty(VideoPlayer.instance.videoQualityOptions.Quality4K.Url))
                options.Add("4K");

            if (VideoPlayer.instance.videoQualityOptions.Quality2K != null && !string.IsNullOrEmpty(VideoPlayer.instance.videoQualityOptions.Quality2K.Url))
                options.Add("2K");
                
            if (VideoPlayer.instance.videoQualityOptions.AdaptiveStream != null && !string.IsNullOrEmpty(VideoPlayer.instance.videoQualityOptions.AdaptiveStream.Url))
                options.Add("Adaptive");
        }
        
        if (options.Count == 0)
            options.Add("Default");
            
        qualityDropdown.AddOptions(options);
    }

    /// <summary>
    /// Call this method when the current video changes to update the quality options
    /// </summary>
    public void UpdateQualityOptions()
    {
        SetupQualityDropdown();
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
    /// Checks if the settings panel is currently visible
    /// </summary>
    public bool IsSettingsPanelVisible()
    {
        return isSettingsPanelVisible;
    }
    
    /// <summary>
    /// Shows or hides the settings panel directly without animation
    /// </summary>
    public void SetSettingsPanelVisible(bool visible)
    {
        if (settingsPanelCanvasGroup == null)
            return;
            
        isSettingsPanelVisible = visible;
        settingsPanelCanvasGroup.alpha = visible ? 1f : 0f;
        settingsPanelCanvasGroup.interactable = visible;
        settingsPanelCanvasGroup.blocksRaycasts = visible;
    }

    // IAutoHideUI Implementation
    public CanvasGroup GetCanvasGroup()
    {
        return settingsPanelCanvasGroup;
    }

    public void OnShow()
    {
        isSettingsPanelVisible = true;
        if (messageText != null)
        {
            ShowMessage("Settings Panel");
        }
    }

    public void OnHide()
    {
        isSettingsPanelVisible = false;
    }

    public void OnInteract()
    {
        // Additional interaction handling if needed
    }
}
