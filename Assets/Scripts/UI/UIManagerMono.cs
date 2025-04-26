using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIManagerMono : MonoBehaviour
{
    public static UIManagerMono instance;

    [SerializeField] private CanvasGroup videoListPanel;
    [SerializeField] private CanvasGroup selectedVideoPanel;
    void Awake()
    {
        if(instance != null)
            Destroy(instance.gameObject);
        
        instance = this;
    }

    public void ShowVideoListPanel()
    {
        ShowPanel(videoListPanel);
        HidePanel(selectedVideoPanel);
    }

    public void ShowSelectedVideoPanel()
    {
        ShowPanel(selectedVideoPanel);
        HidePanel(videoListPanel);
    }
    
    
    internal void TogglePanel(CanvasGroup panel)
    {
        if(panel.alpha == 1)
            HidePanel(panel);
        else
            ShowPanel(panel);
    }
    

    private void ShowPanel(CanvasGroup panel)
    {
        panel.alpha = 1;
        panel.blocksRaycasts = true;
        panel.interactable = true;
    }

    private void HidePanel(CanvasGroup panel)
    {
        panel.alpha = 0;
        panel.blocksRaycasts = false;
        panel.interactable = false;
    }
}
