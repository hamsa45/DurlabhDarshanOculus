using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using TMPro;

/// <summary>
/// An enhanced IAP manager for Meta Quest using the Meta XR Platform SDK
/// Supports both single-product purchases and multi-product listing
/// Uses in-memory storage instead of persistent storage
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

    public static MetaIAPManager Instance { get; private set; }

    // List of available IAP products
    public List<IAPProduct> availableProducts = new List<IAPProduct>();

    // Dictionary for quick product lookup by SKU
    private Dictionary<string, IAPProduct> productDictionary = new Dictionary<string, IAPProduct>();

    // In-memory storage for owned products (replaces PlayerPrefs)
    private HashSet<string> ownedProducts = new HashSet<string>();

    // Product being purchased
    private string currentPurchaseSKU;

    // Initialization status
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    [Header("Loading Indicator")]
    [Tooltip("Loading indicator object to show during purchase")]
    public GameObject loadingIndicator;
    [Tooltip("Optional status text to display messages")]
    public TextMeshProUGUI statusText;

    // Events
    public event Action<List<IAPProduct>> OnProductsFetched;
    public event Action<IAPProduct> OnSingleProductFetched;
    public event Action<string> OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;
    public event Action<string> OnInitializeFailed;
    public event Action OnInitializeSuccess;

    [Header("Configuration")]
    [SerializeField] private bool initializeOnAwake = true;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (initializeOnAwake)
            {
                Initialize();
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Initialize the IAP system
    /// </summary>
    public void Initialize()
    {
        // Clear owned products on initialization
        ownedProducts.Clear();

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

        isInitialized = true;
        OnInitializeSuccess?.Invoke();
    }

    /// <summary>
    /// Fetch available IAP products
    /// </summary>
    public void FetchAvailableProducts(string[] skus)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Trying to fetch products before initialization. Queue this request.");
            OnInitializeSuccess += () => FetchAvailableProducts(skus);
            return;
        }

        IAP.GetProductsBySKU(skus).OnComplete(GetProductsCallback);
    }

    /// <summary>
    /// Callback for retrieving a single product
    /// </summary>
    void GetSingleProductCallback(Message<ProductList> message)
    {
        if (message.IsError)
        {
            Debug.LogError($"Failed to get product: {message.GetError().Message}");
            return;
        }

        if (message.Data.Count == 0)
        {
            Debug.LogError("No products returned");
            return;
        }

        Product product = message.Data[0];
        IAPProduct iapProduct = new IAPProduct
        {
            SKU = product.Sku,
            Name = product.Name,
            Description = product.Description,
            Price = (float)product.Price.AmountInHundredths / 100f,
            FormattedPrice = product.FormattedPrice
        };

        // Cache it
        productDictionary[product.Sku] = iapProduct;

        // Notify listeners
        OnSingleProductFetched?.Invoke(iapProduct);
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
            AccountPage.ap.SetSubscriptionStatus(true);
            AccountPage.ap.SetUpSubscriptionText(purchase.ExpirationTime.ToString("dd-mm-yyyy"));
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
        if (!isInitialized)
        {
            Debug.LogWarning("Trying to purchase before initialization. Queue this request.");
            OnInitializeSuccess += () => PurchaseProduct(sku);
            return;
        }

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
        // Store the purchase in memory only
        ownedProducts.Add(sku);

        Debug.Log($"Item {sku} granted to user (stored in memory)");
    }

    /// <summary>
    /// Check if user owns a specific product
    /// </summary>
    /// <param name="sku">The SKU to check</param>
    /// <returns>True if the user owns the product</returns>
    public bool DoesUserOwnProduct(string sku)
    {
        return ownedProducts.Contains(sku);
    }

    /// <summary>
    /// Get all owned products
    /// </summary>
    /// <returns>A copy of the owned products set</returns>
    public HashSet<string> GetOwnedProducts()
    {
        return new HashSet<string>(ownedProducts);
    }

    /// <summary>
    /// Manually add a product to owned products (for testing or special cases)
    /// </summary>
    /// <param name="sku">The SKU to add</param>
    public void AddOwnedProduct(string sku)
    {
        ownedProducts.Add(sku);
        Debug.Log($"Manually added {sku} to owned products");
    }

    /// <summary>
    /// Remove a product from owned products (for testing or special cases)
    /// </summary>
    /// <param name="sku">The SKU to remove</param>
    public void RemoveOwnedProduct(string sku)
    {
        ownedProducts.Remove(sku);
        Debug.Log($"Removed {sku} from owned products");
    }

    /// <summary>
    /// Clear all owned products
    /// </summary>
    public void ClearOwnedProducts()
    {
        ownedProducts.Clear();
        Debug.Log("Cleared all owned products from memory");
    }

    /// <summary>
    /// Manually restore purchases from server
    /// </summary>
    public void RestorePurchases()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Trying to restore purchases before initialization. Queue this request.");
            OnInitializeSuccess += RestorePurchases;
            return;
        }

        IAP.GetViewerPurchases().OnComplete(GetPurchasesCallback);
    }
}