using UnityEngine;
using System;

public class AutoHideUIComponent : MonoBehaviour
{
    [Header("Auto Hide Settings")]
    [SerializeField] private float autoHideDelay = 3f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private bool startHidden = true;

    private IAutoHideUI targetUI;
    private float hideTimer = 0f;
    private bool isTimerRunning = false;
    private bool isVisible = false;

    private void Awake()
    {
        targetUI = GetComponent<IAutoHideUI>();
        if (targetUI == null)
        {
            Debug.LogError($"No IAutoHideUI component found on {gameObject.name}");
            enabled = false;
            return;
        }

        if (startHidden)
        {
            Hide();
        }
    }

    private void Update()
    {
        if (isTimerRunning && isVisible)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0f)
            {
                Hide();
            }
        }
    }

    /// <summary>
    /// Shows the UI element and starts the auto-hide timer
    /// </summary>
    public void Show()
    {
        if (!isVisible)
        {
            isVisible = true;
            UIFadeManager.Instance.FadeIn(targetUI.GetCanvasGroup(), fadeDuration, () => {
                targetUI.OnShow();
                StartHideTimer();
            });
        }
        else
        {
            ResetHideTimer();
        }
    }

    /// <summary>
    /// Hides the UI element and stops the auto-hide timer
    /// </summary>
    public void Hide()
    {
        if (isVisible)
        {
            isVisible = false;
            UIFadeManager.Instance.FadeOut(targetUI.GetCanvasGroup(), fadeDuration, () => {
                targetUI.OnHide();
                StopHideTimer();
            });
        }
    }

    /// <summary>
    /// Called when the UI element is interacted with
    /// </summary>
    public void OnInteract()
    {
        if (isVisible)
        {
            targetUI.OnInteract();
            ResetHideTimer();
        }
    }

    private void StartHideTimer()
    {
        isTimerRunning = true;
        hideTimer = autoHideDelay;
    }

    private void StopHideTimer()
    {
        isTimerRunning = false;
        hideTimer = 0f;
    }

    private void ResetHideTimer()
    {
        hideTimer = autoHideDelay;
    }
} 