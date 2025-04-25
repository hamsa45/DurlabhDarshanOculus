using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowPassword : MonoBehaviour
{
    public TMP_InputField inputField;
    public Toggle toggle;

    private void Start()
    {
        inputField.inputType = TMP_InputField.InputType.Password;
    }

    public void TogglePassword()
    {
        if (toggle.isOn)
            ShowInput();
        else
            HideInput();
    }

    public void ShowInput()
    {
        inputField.inputType = TMP_InputField.InputType.Standard;
        inputField.ForceLabelUpdate();
    }

    public void HideInput()
    {
        inputField.inputType = TMP_InputField.InputType.Password;
        inputField.ForceLabelUpdate();
    }
}
