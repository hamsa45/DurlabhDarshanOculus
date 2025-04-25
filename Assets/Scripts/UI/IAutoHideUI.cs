using UnityEngine;

public interface IAutoHideUI
{
    /// <summary>
    /// Gets the CanvasGroup component of the UI element
    /// </summary>
    CanvasGroup GetCanvasGroup();

    /// <summary>
    /// Called when the UI element should be shown
    /// </summary>
    void OnShow();

    /// <summary>
    /// Called when the UI element should be hidden
    /// </summary>
    void OnHide();

    /// <summary>
    /// Called when the UI element is interacted with
    /// </summary>
    void OnInteract();
} 