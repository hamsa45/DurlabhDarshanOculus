using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manager component for handling fade transitions for UI elements.
/// Attach this to a GameObject in the scene to facilitate all fade transitions.
/// </summary>
public class UIFadeManager : MonoBehaviour
{
    private static UIFadeManager _instance;
    
    public static UIFadeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find existing instance in scene
                _instance = FindObjectOfType<UIFadeManager>();
                
                // If no instance exists, create a new one
                if (_instance == null)
                {
                    GameObject go = new GameObject("UIFadeManager");
                    _instance = go.AddComponent<UIFadeManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Fades a CanvasGroup to a target alpha value
    /// </summary>
    /// <param name="canvasGroup">The CanvasGroup to fade</param>
    /// <param name="targetAlpha">The target alpha value (0-1)</param>
    /// <param name="duration">Duration of the fade transition in seconds</param>
    /// <param name="onComplete">Optional callback when the fade completes</param>
    /// <returns>The coroutine handle</returns>
    public Coroutine Fade(CanvasGroup canvasGroup, float targetAlpha, float duration, Action onComplete = null)
    {
        if (canvasGroup == null)
        {
            Debug.LogError("UIFadeManager: CanvasGroup is null");
            return null;
        }

        return StartCoroutine(FadeCoroutine(canvasGroup, targetAlpha, duration, onComplete));
    }

    /// <summary>
    /// Fades a CanvasGroup to a fully visible state (alpha = 1)
    /// </summary>
    /// <param name="canvasGroup">The CanvasGroup to fade in</param>
    /// <param name="duration">Duration of the fade transition in seconds</param>
    /// <param name="onComplete">Optional callback when the fade completes</param>
    /// <returns>The coroutine handle</returns>
    public Coroutine FadeIn(CanvasGroup canvasGroup, float duration, Action onComplete = null)
    {
        return Fade(canvasGroup, 1f, duration, onComplete);
    }

    /// <summary>
    /// Fades a CanvasGroup to a fully invisible state (alpha = 0)
    /// </summary>
    /// <param name="canvasGroup">The CanvasGroup to fade out</param>
    /// <param name="duration">Duration of the fade transition in seconds</param>
    /// <param name="onComplete">Optional callback when the fade completes</param>
    /// <returns>The coroutine handle</returns>
    public Coroutine FadeOut(CanvasGroup canvasGroup, float duration, Action onComplete = null)
    {
        return Fade(canvasGroup, 0f, duration, onComplete);
    }

    /// <summary>
    /// The coroutine that handles the fade transition
    /// </summary>
    private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float targetAlpha, float duration, Action onComplete)
    {
        // Always ensure the GameObject is active at the start of fade transitions
        if (!canvasGroup.gameObject.activeSelf)
        {
            canvasGroup.gameObject.SetActive(true);
        }

        float startAlpha = canvasGroup.alpha;
        float time = 0f;
        
        // If duration is zero or very small, just set the final values immediately
        if (duration <= 0.01f)
        {
            canvasGroup.alpha = targetAlpha;
            UpdateCanvasGroupInteractivity(canvasGroup, targetAlpha);
            
            if (onComplete != null)
            {
                onComplete.Invoke();
            }
            
            yield break;
        }

        while (time < duration)
        {
            time += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(time / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, normalizedTime);
            yield return null;
        }
        
        // Ensure we reach the exact target alpha
        canvasGroup.alpha = targetAlpha;
        
        // Update interactivity based on alpha
        UpdateCanvasGroupInteractivity(canvasGroup, targetAlpha);
        
        // If the element is now fully transparent, we can disable the GameObject to save resources
        if (targetAlpha <= 0)
        {
            canvasGroup.gameObject.SetActive(false);
        }
        
        // Call the completion callback if provided
        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }
    
    /// <summary>
    /// Updates the interactivity properties of a CanvasGroup based on alpha value
    /// </summary>
    private void UpdateCanvasGroupInteractivity(CanvasGroup canvasGroup, float alpha)
    {
        bool isInteractive = alpha > 0;
        canvasGroup.interactable = isInteractive;
        canvasGroup.blocksRaycasts = isInteractive;
    }
} 