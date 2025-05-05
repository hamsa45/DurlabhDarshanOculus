using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class NavbarManager : MonoBehaviour
{
    [Tooltip("List of all navbar buttons")]
    public List<Button> navbarButtons = new();

    [Tooltip("Index of the button that should be initially selected (0-4)")]
    public int initialSelectedButton = 0;

    private void Start()
    {
        QualitySettings.antiAliasing = 4;
        XRSettings.eyeTextureResolutionScale = 1.2f; // Slight supersampling

        // Set initial button state
        SetButtonStates(initialSelectedButton);

        // Add click listeners to all buttons
        for (int i = 0; i < navbarButtons.Count; i++)
        {
            int buttonIndex = i; // Capture the index for the lambda
            navbarButtons[i].onClick.AddListener(() => HandleButtonClick(buttonIndex));
        }
    }

    /// <summary>
    /// Handles button click events
    /// </summary>
    /// <param name="clickedButtonIndex">Index of the clicked button</param>
    public void HandleButtonClick(int clickedButtonIndex)
    {
        // Set all buttons' interactable states
        SetButtonStates(clickedButtonIndex);
    }

    /// <summary>
    /// Sets the interactable state of all buttons based on the selected button
    /// </summary>
    /// <param name="selectedButtonIndex">Index of the selected button</param>
    private void SetButtonStates(int selectedButtonIndex)
    {
        // Make sure index is valid
        if (selectedButtonIndex < 0 || selectedButtonIndex >= navbarButtons.Count)
        {
            Debug.LogError($"Invalid button index: {selectedButtonIndex}");
            return;
        }

        // Set all buttons to interactable=true
        for (int i = 0; i < navbarButtons.Count; i++)
        {
            navbarButtons[i].interactable = true;
        }

        // Set the selected button to interactable=false
        navbarButtons[selectedButtonIndex].interactable = false;
    }

    /// <summary>
    /// Public method to select a specific button by index
    /// Can be called from other scripts
    /// </summary>
    /// <param name="buttonIndex">Index of the button to select (0-4)</param>
    public void SelectButton(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < navbarButtons.Count)
        {
            SetButtonStates(buttonIndex);
        }
        else
        {
            Debug.LogWarning($"Attempted to select invalid button index: {buttonIndex}");
        }
    }
}