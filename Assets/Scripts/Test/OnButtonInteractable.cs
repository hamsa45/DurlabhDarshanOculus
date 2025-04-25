using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class OnButtonInteractable : MonoBehaviour
{
    [SerializeField]
    private Button button;

    [Space]

    [SerializeField]
    private UnityEvent onClickEvent;

    [SerializeField]
    private UnityEvent onInteractableEvent;

    internal Button GetButton() => button;

    private void OnEnable()
    {
        button.onClick.AddListener(() => OnButtonClick());
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    internal void OnButtonClick()
    {
        ButtonInteraction(false);
        if (onClickEvent != null)
        {
            onClickEvent.Invoke();
        }
    }

    internal void TriggerExternalEvent()
    {
        ButtonInteraction(true);
        if (onInteractableEvent != null)
        {
            onInteractableEvent.Invoke();
        }
    }

    private void ButtonInteraction(bool isInteractale) => button.interactable = isInteractale;
}