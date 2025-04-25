using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using RenderHeads.Media.AVProVideo;

/// <summary>
/// Handles drag events for a video player slider to improve user experience
/// </summary>
public class SliderDragHandler : MonoBehaviour
{
    public Slider slider;
    public MediaPlayer mediaPlayer;
    
    // Event that fires when slider drag begins
    public event Action OnDragBegin;
    
    // Event that fires when slider drag ends with the final value
    public event Action<float> OnDragEnd;
    
    // Auto-find components if not assigned
    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
            
        // Add event triggers
        AddEventTriggers();
    }
    
    private void AddEventTriggers()
    {
        // Need EventTrigger component
        EventTrigger eventTrigger = slider.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = slider.gameObject.AddComponent<EventTrigger>();
            
        // Clear existing triggers to avoid duplicates
        eventTrigger.triggers.Clear();
        
        // Add pointer down event (start dragging)
        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { 
            OnDragBegin?.Invoke();
        });
        eventTrigger.triggers.Add(pointerDown);
        
        // Add pointer up event (stop dragging)
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => {
            OnDragEnd?.Invoke(slider.value);
            
            // If media player is assigned, seek to the position
            if (mediaPlayer != null && mediaPlayer.Info != null && mediaPlayer.Control != null)
            {
                double newTime = slider.value * mediaPlayer.Info.GetDuration();
                mediaPlayer.Control.Seek(newTime);
            }
        });
        eventTrigger.triggers.Add(pointerUp);
    }
} 