using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewButtonManager : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform contentPanel;
    [SerializeField] private RectTransform[] sectionRects; // 0: Gallery, 1: About, 2: Reviews
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private float scrollSpeed = 10f;

    private float targetNormalizedPos;
    private bool isScrolling = false;
    private int currentSelectedIndex;

    private float lastScrollPosition = 0f;
    private bool userScrolling = false;
    private float scrollStopTimer = 0f;
    private float scrollStopThreshold = 0.3f;

    void Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int idx = i;
            tabButtons[i].onClick.AddListener(() => ScrollToSection(idx));
        }
    }

    private void Update()
    {
        // Smooth scrolling when triggered by button click
        if (isScrolling)
        {
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(
                scrollRect.verticalNormalizedPosition,
                targetNormalizedPos,
                Time.deltaTime * scrollSpeed
            );

            if (Mathf.Abs(scrollRect.verticalNormalizedPosition - targetNormalizedPos) < 0.001f)
            {
                isScrolling = false;
            }
        }

        // Only run manual scroll detection if not auto-scrolling
        if (!isScrolling)
        {
            if (Mathf.Abs(scrollRect.normalizedPosition.y - lastScrollPosition) > 0.001f)
            {
                userScrolling = true;
                scrollStopTimer = 0f;
            }

            if (userScrolling)
            {
                scrollStopTimer += Time.deltaTime;
                if (scrollStopTimer > scrollStopThreshold)
                {
                    userScrolling = false;
                    UpdateButtonSelectionFromManualScroll(); // Uses posY-based logic
                }
            }

            lastScrollPosition = scrollRect.normalizedPosition.y;
        }
    }


    public void ScrollToSection(int index)
    {
        float contentHeight = contentPanel.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        float targetPos;

        if (index == 0)
        {
            // Scroll to top
            targetPos = 1f;
        }
        else if (index == sectionRects.Length - 1)
        {
            // Scroll to bottom
            targetPos = 0f;
        }
        else
        {
            // Middle section: scroll that section to top
            RectTransform section = sectionRects[index];
            float sectionPosY = Mathf.Abs(section.localPosition.y);

            targetPos = 1f - (sectionPosY / (contentHeight - viewportHeight));
            targetPos = Mathf.Clamp01(targetPos);
        }

        targetNormalizedPos = targetPos;
        isScrolling = true;

        // Optional: Highlight selected tab
        for (int i = 0; i < tabButtons.Length; i++)
        {
            tabButtons[i].interactable = (i != index);
        }

        currentSelectedIndex = index;
    }

    private void UpdateButtonSelectionFromManualScroll()
    {
        float threshold = 0.01f;

        // Detect top or bottom scroll
        float norm = scrollRect.verticalNormalizedPosition;
        if (norm >= 1f - threshold)
        {
            SetSelectedIndex(0);
            return;
        }
        if (norm <= threshold)
        {
            SetSelectedIndex(sectionRects.Length - 1);
            return;
        }

        // Compare each section's top position to viewport's top (local y = 0)
        float closestDistance = float.MaxValue;
        int closestIndex = currentSelectedIndex;

        for (int i = 0; i < sectionRects.Length; i++)
        {
            // Convert section's world position to viewport-local space
            Vector3 localPos = scrollRect.viewport.InverseTransformPoint(sectionRects[i].position);

            // Since viewport top is at y = 0, compare distance from that
            float distanceFromTop = Mathf.Abs(localPos.y); // Want y closest to 0 (top)

            if (distanceFromTop < closestDistance)
            {
                closestDistance = distanceFromTop;
                closestIndex = i;
            }
        }

        SetSelectedIndex(closestIndex);
    }


    private void SetSelectedIndex(int index)
    {
        if (index != currentSelectedIndex)
        {
            currentSelectedIndex = index;
            for (int i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i].interactable = (i != index);
            }
        }
    }


}
