using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Toast;
    /// <summary>
    /// ////updated
    /// </summary>
    public enum ToastType { Success, Info, Error }
    [Header ("Toast Panels")]
    public GameObject InfoToastObject;
    public GameObject SuccessToastObject;
    public GameObject ErrorToastObject;

    private ToastPanel toastpanel;

    private void Start()
    {
        Toast = this;
        
    }

    private void SelectToaster(GameObject toast, float toastTime, string text = "")
    {
        toast.transform.SetSiblingIndex(2);
        toast.SetActive(false);
        toast.SetActive(true);
        toastpanel = toast.GetComponent<ToastPanel>();
        if(toastpanel is not null)
            toastpanel.ChangeSubheading(text, toastTime);
    }

    public void SuccessToast(string text, float time = 3f) => ToastMessage(ToastType.Success, text, time);
    public void ErrorToast(string text, float time = 3f) => ToastMessage(ToastType.Error, text, time);
    public void InfoToast(string text, float time = 3f) => ToastMessage(ToastType.Info, text, time);

    private void ToastMessage(ToastType toast, string text, float time)
    {
        switch (toast)
        {
            case ToastType.Info:
                SelectToaster(InfoToastObject, time, text);
                break;
            case ToastType.Success:
                SelectToaster(SuccessToastObject, time, text);
                break;
            case ToastType.Error:
                SelectToaster(ErrorToastObject, time, text);
                break;
            default:
                SelectToaster(InfoToastObject, time, text);
                break;
        }
    }

    public void DisableAllToast()
    {
        InfoToastObject.SetActive(false);
        SuccessToastObject.SetActive(false);
        ErrorToastObject.SetActive(false);
    }

}
