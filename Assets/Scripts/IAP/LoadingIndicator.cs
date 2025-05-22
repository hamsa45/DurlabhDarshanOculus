using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A simple loading indicator component that can be shown during IAP operations.
/// </summary>
public class LoadingIndicator : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The visual loading animation (spinner, etc.)")]
    public GameObject loadingAnimation;
    [Tooltip("Optional text to display loading messages")]
    public TextMeshProUGUI loadingText;
    [Tooltip("Optional background panel")]
    public GameObject backgroundPanel;
    
    [Header("Animation Settings")]
    [Tooltip("Should the loading animation rotate?")]
    public bool rotateAnimation = true;
    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 180f;
    
    private bool isVisible = false;
    
    void Start()
    {
        // Hide at start
        Hide();
    }
    
    void Update()
    {
        // Rotate the loading animation if enabled
        if (isVisible && rotateAnimation && loadingAnimation != null)
        {
            loadingAnimation.transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Show the loading indicator with an optional message
    /// </summary>
    /// <param name="message">Message to display (if null, keeps current message)</param>
    public void Show(string message = null)
    {
        isVisible = true;
        
        // Show all components
        if (backgroundPanel != null)
            backgroundPanel.SetActive(true);
            
        if (loadingAnimation != null)
            loadingAnimation.SetActive(true);
            
        // Update text if provided
        if (loadingText != null && message != null)
            loadingText.text = message;
            
        // Make sure the game object is active
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Hide the loading indicator
    /// </summary>
    public void Hide()
    {
        isVisible = false;
        
        // You can either hide the entire object or just its components
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Update the loading message
    /// </summary>
    /// <param name="message">New message to display</param>
    public void UpdateMessage(string message)
    {
        if (loadingText != null)
            loadingText.text = message;
    }
    
    /// <summary>
    /// Show the loading indicator for a specific duration
    /// </summary>
    /// <param name="duration">Time to show in seconds</param>
    /// <param name="message">Optional message to display</param>
    public void ShowForDuration(float duration, string message = null)
    {
        StartCoroutine(ShowThenHide(duration, message));
    }
    
    private IEnumerator ShowThenHide(float duration, string message)
    {
        Show(message);
        yield return new WaitForSeconds(duration);
        Hide();
    }
}