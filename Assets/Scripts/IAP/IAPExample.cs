using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IAPExample : MonoBehaviour
{
    [Header("UI References")]
    public Transform productContainer;
    public GameObject productButtonPrefab;
    public TextMeshProUGUI statusText;
    
    // [Header("Products Configuration")]
    // public string[] productSKUs = new string[]
    // {
    //     "com.yourcompany.yourgame.item1",
    //     "com.yourcompany.yourgame.item2",
    //     "com.yourcompany.yourgame.removeads"
    // };

    void Start()
    {
        // Subscribe to IAP events
        MetaIAPManager.Instance.OnProductsFetched += OnProductsFetched;
        MetaIAPManager.Instance.OnPurchaseSuccess += OnPurchaseSuccess;
        MetaIAPManager.Instance.OnPurchaseFailed += OnPurchaseFailed;
        MetaIAPManager.Instance.OnInitializeFailed += OnInitFailed;
        
        // Set the product SKUs in the IAP Manager
        // You would typically set these in the Inspector
        
        // Fetch products after a short delay to ensure platform is initialized
        StartCoroutine(FetchProductsAfterDelay());
    }

    IEnumerator FetchProductsAfterDelay()
    {
        statusText.text = "Initializing Meta IAP...";
        yield return new WaitForSeconds(1.0f);
        
        statusText.text = "Fetching products...";
        MetaIAPManager.Instance.FetchAvailableProducts();
    }

    void OnProductsFetched(List<MetaIAPManager.IAPProduct> products)
    {
        statusText.text = $"Found {products.Count} products";
        
        // Clear existing product buttons
        foreach (Transform child in productContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create a button for each product
        foreach (var product in products)
        {
            GameObject buttonObj = Instantiate(productButtonPrefab, productContainer);
            
            // Set button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = $"{product.Name} - {product.FormattedPrice}";
            }
            
            // Set button click action
            Button button = buttonObj.GetComponent<Button>();
            string productSKU = product.SKU; // Create a local copy for the lambda
            button.onClick.AddListener(() => PurchaseProduct(productSKU));
            
            // Disable button if already owned
            if (MetaIAPManager.Instance.DoesUserOwnProduct(product.SKU))
            {
                button.interactable = false;
                if (buttonText != null)
                {
                    buttonText.text = $"{product.Name} - Owned";
                }
            }
        }
    }

    public void PurchaseProduct(string sku)
    {
        statusText.text = $"Purchasing {sku}...";
        MetaIAPManager.Instance.PurchaseProduct(sku);
    }

    void OnPurchaseSuccess(string sku)
    {
        statusText.text = $"Successfully purchased {sku}!";
        
        // Refresh the product list to update the UI
        MetaIAPManager.Instance.FetchAvailableProducts();
        
        // Apply the purchase effect (remove ads, add coins, etc.)
        ApplyPurchaseEffect(sku);
    }

    void OnPurchaseFailed(string errorMsg)
    {
        statusText.text = $"Purchase failed: {errorMsg}";
    }
    
    void OnInitFailed(string errorMsg)
    {
        statusText.text = $"Meta IAP initialization failed: {errorMsg}";
    }
    
    void ApplyPurchaseEffect(string sku)
    {
        // Implement your game-specific purchase effects here
        switch (sku)
        {
            case "com.yourcompany.yourgame.item1":
                // Give the player 100 coins
                Debug.Log("Giving player 100 coins");
                break;
                
            case "com.yourcompany.yourgame.item2":
                // Unlock premium content
                Debug.Log("Unlocking premium content");
                break;
                
            case "com.yourcompany.yourgame.removeads":
                // Remove ads
                Debug.Log("Removing ads");
                break;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (MetaIAPManager.Instance != null)
        {
            MetaIAPManager.Instance.OnProductsFetched -= OnProductsFetched;
            MetaIAPManager.Instance.OnPurchaseSuccess -= OnPurchaseSuccess;
            MetaIAPManager.Instance.OnPurchaseFailed -= OnPurchaseFailed;
            MetaIAPManager.Instance.OnInitializeFailed -= OnInitFailed;
        }
    }
}
