using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;

/// <summary>
/// A basic IAP manager for Meta Quest using the Meta XR Platform SDK
/// </summary>
public class MetaIAPManager : MonoBehaviour
{
    [Serializable]
    public class IAPProduct
    {
        public string SKU;
        public string Name;
        public string Description;
        public float Price;
        public string FormattedPrice;
    }

    // Singleton instance
    public static MetaIAPManager Instance { get; private set; }

    // List of available IAP products
    public List<IAPProduct> availableProducts = new List<IAPProduct>();

    // Product being purchased
    private string currentPurchaseSKU;

    // Events
    public event Action<List<IAPProduct>> OnProductsFetched;
    public event Action<string> OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;
    public event Action<string> OnInitializeFailed;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Initialize the platform SDK
        try
        {
            Core.Initialize();
            Debug.Log("Meta XR Platform SDK initialized successfully");
            
            // Register callbacks for IAP
            Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Meta XR Platform SDK: {e.Message}");
            OnInitializeFailed?.Invoke(e.Message);
        }
    }

    /// <summary>
    /// Callback for user entitlement check
    /// </summary>
    void EntitlementCallback(Message message)
    {
        if (message.IsError)
        {
            Debug.LogError($"Entitlement check failed: {message.GetError().Message}");
            OnInitializeFailed?.Invoke(message.GetError().Message);
            return;
        }

        Debug.Log("User is entitled to use this application");
        
        // Set up IAP callbacks
        IAP.GetViewerPurchases().OnComplete(GetPurchasesCallback);
    }

    /// <summary>
    /// Fetch available IAP products
    /// </summary>
    public void FetchAvailableProducts()
    {
        IAP.GetProductsBySKU(new string[] { /* Add your SKUs here */ }).OnComplete(GetProductsCallback);
    }

    /// <summary>
    /// Callback for retrieving available products
    /// </summary>
    void GetProductsCallback(Message<ProductList> message)
    {
        if (message.IsError)
        {
            Debug.LogError($"Failed to get products: {message.GetError().Message}");
            return;
        }

        availableProducts.Clear();
        foreach (Product product in message.Data)
        {
            IAPProduct iapProduct = new IAPProduct
            {
                SKU = product.Sku,
                Name = product.Name,
                Description = product.Description,
                Price = (float)product.Price.AmountInHundredths,
                FormattedPrice = product.FormattedPrice
            };
            availableProducts.Add(iapProduct);
        }

        Debug.Log($"Fetched {availableProducts.Count} products");
        OnProductsFetched?.Invoke(availableProducts);
    }

    /// <summary>
    /// Get user's previous purchases
    /// </summary>
    void GetPurchasesCallback(Message<PurchaseList> message)
    {
        if (message.IsError)
        {
            Debug.LogError($"Failed to get purchases: {message.GetError().Message}");
            return;
        }

        Debug.Log($"User has {message.Data.Count} previous purchases");
        foreach (Purchase purchase in message.Data)
        {
            Debug.Log($"User owns: {purchase.Sku}");
            // Process existing purchases (e.g., restore purchases functionality)
            GrantItem(purchase.Sku);
        }
    }

    /// <summary>
    /// Initiate a purchase for a product
    /// </summary>
    /// <param name="sku">The SKU of the product to purchase</param>
    public void PurchaseProduct(string sku)
    {
        currentPurchaseSKU = sku;
        IAP.LaunchCheckoutFlow(sku).OnComplete(ProcessPurchase);
    }

    /// <summary>
    /// Callback when a purchase is completed via LaunchCheckoutFlow
    /// </summary>
    void ProcessPurchase(Message<Purchase> message)
    {
        if (message.IsError)
        {
            Debug.LogError($"Purchase failed: {message.GetError().Message}");
            OnPurchaseFailed?.Invoke(message.GetError().Message);
            currentPurchaseSKU = null;
            return;
        }

        HandleSuccessfulPurchase(message.Data);
    }
    
    /// <summary>
    /// Common handler for successful purchases
    /// </summary>
    private void HandleSuccessfulPurchase(Purchase purchase)
    {
        if (purchase != null)
        {
            Debug.Log($"Successfully purchased {purchase.Sku}");
            
            // Grant the item to the user
            GrantItem(purchase.Sku);
            
            OnPurchaseSuccess?.Invoke(purchase.Sku);
            currentPurchaseSKU = null;
        }
    }

    /// <summary>
    /// Grant the purchased item to the user
    /// </summary>
    /// <param name="sku">The SKU of the purchased item</param>
    private void GrantItem(string sku)
    {
        // Implement your logic to grant the item to the user
        // For example:
        // 1. Unlock content
        // 2. Add virtual currency
        // 3. Enable features
        
        Debug.Log($"Item {sku} granted to user");
        
        // You might want to store the purchase in PlayerPrefs or on a server
        PlayerPrefs.SetInt($"Purchase_{sku}", 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Check if user owns a specific product
    /// </summary>
    /// <param name="sku">The SKU to check</param>
    /// <returns>True if the user owns the product</returns>
    public bool DoesUserOwnProduct(string sku)
    {
        // Simple local check - for a real app, you might want to verify with Oculus servers
        return PlayerPrefs.GetInt($"Purchase_{sku}", 0) == 1;
    }
}