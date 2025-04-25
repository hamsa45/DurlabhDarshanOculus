using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager<T> : MonoBehaviour where T : class
{
    public static GridManager<T> instance;
    
    [Header("Configuration")]
    [SerializeField] private int itemsPerPage = 5;
    [SerializeField] private ThumbnailPanel<T> thumbnailPrefab;
    [SerializeField] private Transform thumbnailContainer;

    [Header("Navigation")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private TMPro.TextMeshProUGUI pageIndicator;

    private List<ThumbnailPanel<T>> thumbnailPanels = new List<ThumbnailPanel<T>>();
    private List<T> itemData;
    private int currentPage = 0;
    private int totalPages = 0;

    private void Awake()
    {
        instance = this;
        InitializeThumbnailPanels();

        if (nextButton) nextButton.onClick.AddListener(NextPage);
        if (previousButton) previousButton.onClick.AddListener(PreviousPage);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) NextPage();
        if (Input.GetKeyDown(KeyCode.Y)) PreviousPage();
    }

    private void InitializeThumbnailPanels()
    {
        for (int i = 0; i < itemsPerPage; i++)
        {
            ThumbnailPanel<T> panel = Instantiate(thumbnailPrefab, thumbnailContainer);
            thumbnailPanels.Add(panel);
            panel.gameObject.SetActive(false);
        }
    }

    public void LoadItems(List<T> data)
    {
        itemData = data;
        currentPage = 0;
        totalPages = Mathf.CeilToInt((float)data.Count / itemsPerPage);

        RefreshItems();
        UpdateNavigationButtons();
    }

    private void RefreshItems()
    {
        foreach (var panel in thumbnailPanels)
        {
            panel.Clear();
            panel.gameObject.SetActive(false);
        }

        int startIndex = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, itemData.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            int panelIndex = i - startIndex;
            thumbnailPanels[panelIndex].gameObject.SetActive(true);
            thumbnailPanels[panelIndex].SetData(itemData[i]);
        }

        if (pageIndicator)
        {
            pageIndicator.text = $"{currentPage + 1} of {totalPages}";
        }
    }

    private void UpdateNavigationButtons()
    {
        if (previousButton)
            previousButton.interactable = currentPage > 0;

        if (nextButton)
            nextButton.interactable = currentPage < totalPages - 1;
    }

    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            RefreshItems();
            UpdateNavigationButtons();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshItems();
            UpdateNavigationButtons();
        }
    }

    public void GoToPage(int pageNumber)
    {
        if (pageNumber >= 0 && pageNumber < totalPages)
        {
            currentPage = pageNumber;
            RefreshItems();
            UpdateNavigationButtons();
        }
    }
}
