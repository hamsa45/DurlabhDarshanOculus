using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NavBarButtonHandler : MonoBehaviour
{
    [System.Serializable]
    private class NavBarButton
    {
        public Button button;
        public GameObject panel;
    }

    [SerializeField] private List<NavBarButton> navBarButtons;

    private void Start()
    {
        foreach (var button in navBarButtons)
        {
            button.button.onClick.AddListener(() => ShowPanel(button.button));
        }

        // Load and show the last selected panel, or default to index 0
        int lastSelectedIndex = PlayerPrefsManager.LoadSelectedNavPanel();
        if (lastSelectedIndex >= 0 && lastSelectedIndex < navBarButtons.Count)
        {
            ShowPanel(navBarButtons[lastSelectedIndex].button);
        }
        else
        {
            ShowPanel(navBarButtons[0].button);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.V)){
            navBarButtons[1].button.onClick.Invoke();
        }
        else if(Input.GetKeyDown(KeyCode.B)){
            navBarButtons[2].button.onClick.Invoke();
        }
    }

    private void ShowPanel(Button panel)
    {
        for (int i = 0; i < navBarButtons.Count; i++)
        {
            OnButtonInteractable buttonInteractable = navBarButtons[i].button.GetComponent<OnButtonInteractable>();
            if (navBarButtons[i].button == panel)
            {
                navBarButtons[i].button.interactable = false;
                navBarButtons[i].panel?.SetActive(true);
                buttonInteractable?.OnButtonClick();
                // Save the selected navigation panel index
                PlayerPrefsManager.SaveSelectedNavPanel(i);
            }
            else
            {
                navBarButtons[i].button.interactable = true;
                navBarButtons[i].panel?.SetActive(false);
                buttonInteractable?.TriggerExternalEvent();
            } 
        }
    }

    public void OnClickNavBarButton(Button panel)
    {
        for (int i = 0; i < navBarButtons.Count; i++)
        {
            OnButtonInteractable buttonInteractable = navBarButtons[i].button.GetComponent<OnButtonInteractable>();
            if(navBarButtons[i].button == panel){
                navBarButtons[i].button.interactable = false;
                navBarButtons[i].panel?.SetActive(true);
                buttonInteractable?.OnButtonClick();
                // Save the selected navigation panel index
                PlayerPrefsManager.SaveSelectedNavPanel(i);
            }
            else{
                navBarButtons[i].button.interactable = true;
                navBarButtons[i].panel?.SetActive(false);
                buttonInteractable?.TriggerExternalEvent();
            }
        }
    }
}

