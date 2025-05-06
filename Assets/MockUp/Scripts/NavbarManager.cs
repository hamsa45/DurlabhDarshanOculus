using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class NavbarManager : MonoBehaviour
{
    [Tooltip("List of all navbar buttons")]
    public List<Button> navbarButtons = new();
    [Tooltip("List of all content panels mapped to buttons")]
    public List<GameObject> panels = new(); // ✅ New: Panels corresponding to buttons
    [Tooltip("Index of the button that should be initially selected (0-4)")]
    public int initialSelectedButton = 0;

    private void Start()
    {
        QualitySettings.antiAliasing = 4;
        XRSettings.eyeTextureResolutionScale = 1.2f;

        SetButtonStates(initialSelectedButton);

        for (int i = 0; i < navbarButtons.Count; i++)
        {
            int buttonIndex = i;
            navbarButtons[i].onClick.AddListener(() => HandleButtonClick(buttonIndex));
        }
    }

    public void HandleButtonClick(int clickedButtonIndex)
    {
        SetButtonStates(clickedButtonIndex);
    }

    private void SetButtonStates(int selectedButtonIndex)
    {
        if (selectedButtonIndex < 0 || selectedButtonIndex >= navbarButtons.Count)
        {
            Debug.LogError($"Invalid button index: {selectedButtonIndex}");
            return;
        }

        for (int i = 0; i < navbarButtons.Count; i++)
        {
            navbarButtons[i].interactable = true;

            // ✅ Deactivate all panels
            if (i < panels.Count)
                panels[i].SetActive(false);
        }

        navbarButtons[selectedButtonIndex].interactable = false;

        // ✅ Activate selected panel
        if (selectedButtonIndex < panels.Count)
            panels[selectedButtonIndex].SetActive(true);
    }

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
