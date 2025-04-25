using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToastPanel : MonoBehaviour
{
    private TextMeshProUGUI SubHeading;
    private CanvasGroup canvasgroup;
    private Vector3 initialposition;

    private void Awake()
    {
        canvasgroup = GetComponent<CanvasGroup>();
        SubHeading = transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        initialposition = new Vector3(0f, 300f, 0f);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        transform.localPosition += new Vector3(0f, Time.deltaTime * 15, 0f);
    }

    public void ChangeSubheading(string text, float time)
    {
        ResetToast();
        SubHeading.text = text;
        StartCoroutine(DisableToastPanel(time));
    }

    IEnumerator DisableToastPanel(float ToastTime)
    {

        // loop over 1 second
        for (float i = 0; i <= 0.5; i += Time.deltaTime)
        {
            // set i as alpha
            canvasgroup.alpha = i * 2;
            yield return null;
        }

        yield return new WaitForSeconds(ToastTime - 1);

        // loop over 1 second backwards
        for (float i = 1; i >= 0.5f; i -= Time.deltaTime)
        {
            // set i as alpha
            canvasgroup.alpha = i - 0.5f;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ResetToast();
        StopCoroutine("DisableToastPanel");
    }

    private void ResetToast()
    {
        canvasgroup.alpha = 0;
        GetComponent<RectTransform>().anchoredPosition = initialposition;
    }

}