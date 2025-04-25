using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AutoSizeButton : MonoBehaviour
{
    [SerializeField] private ContentSizeFitter observer;
    [SerializeField] private TMPro.TextMeshProUGUI text;
    [SerializeField] private RectTransform targetGUI;
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;

    internal void UpdateText(string _text)
    {
        text.text = _text;
        text.ForceMeshUpdate();
    }

    private void OnEnable() {
        StartCoroutine(AdjustSize());
    }

    IEnumerator AdjustSize() {

        Vector2 delta = observer.GetComponent<RectTransform>().sizeDelta;
        yield return new WaitForEndOfFrame();
        targetGUI.sizeDelta = observer.GetComponent<RectTransform>().sizeDelta;
        targetGUI.sizeDelta += new Vector2(offsetX, offsetY);
    }
}
