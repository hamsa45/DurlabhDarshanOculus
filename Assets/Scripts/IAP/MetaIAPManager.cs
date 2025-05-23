using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using TMPro;

/// <summary>
/// Enhanced IAP manager for Meta Quest with subscription support
/// Handles both one-time purchases and subscriptions with status tracking
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
        public bool IsSubscription;
        public string SubscriptionPeriod; // "monthly", "yearly", etc.
    }

    [Serializable]
    public class SubscriptionStatus
    {
        public string SKU;
        public string Name;
        public bool IsActive;
        public DateTime StartDate;
        public DateTime EndDate;
        public bool IsExpired;
        public bool WillRenew;
        public string Status; // "active", "expired", "cancelled", "pending"

        public int DaysRemaining => IsActive ? Mathf.Max(0, (int)(EndDate - DateTime.Now).TotalDays) : 0;
        public bool IsExpiringSoon => DaysRemaining <= 7 && IsActive;
    }

    public static MetaIAPManager Instance { get; private set; }

    // List of available IAP products
    public List<IAPProduct> availableProducts = new List<IAPProduct>();

    // Current subscription statuses
    public List<SubscriptionStatus> activeSubscriptions = new List<SubscriptionStatus>();

    // Dictionary for quick product lookup by SKU
    private Dictionary<string, IAPProduct> productDictionary = new Dictionary<string, IAPProduct>();

    // Dictionary for subscription status lookup
    private Dictionary<string, SubscriptionStatus> subscriptionDictionary = new Dictionary<string, SubscriptionStatus>();

    // Product being purchased
    private string currentPurchaseSKU;

    // Events for loading states
    public event Action OnSubscriptionCheckStarted;
    public event Action OnSubscriptionCheckCompleted;
    public event Action OnPurchaseStarted;
    public event Action OnPurchaseCompleted;

    // Initialization status
    private bool isInitialized = false;
    public bool IsInitialized => isInitialized;

    [Header("Loading Indicators")]
    [Tooltip("Loading indicator object to show during purchase")]
    public GameObject purchaseLoadingIndicator;
    [Tooltip("Loading indicator object to show during subscription status check")]
    public GameObject subscriptionLoadingIndicator;
    [Tooltip("Optional status text to display messages")]
    public TextMeshProUGUI statusText;

    [Header("Subscription Configuration")]
    [Tooltip("List of subscription SKUs to monitor")]
    public string[] subscriptionSKUs;
    [Tooltip("Auto-refresh subscription status interval in seconds")]
    public float subscriptionRefreshInterval = 300f; // 5 minutes

    // Events
    public event Action<List<IAPProduct>> OnProductsFetched;
    public event Action<IAPProduct> OnSingleProductFetched;
    public event Action<string> OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;
    public event Action<string> OnInitializeFailed;
    public event Action OnInitializeSuccess;

    // Subscription-specific events
    public event Action<List<SubscriptionStatus>> OnSubscriptionStatusUpdated;
    public event Action<SubscriptionStatus> OnSubscriptionActivated;
    public event Action<SubscriptionStatus> OnSubscriptionExpired;
    public event Action<SubscriptionStatus> OnSubscriptionCancelled;

    [Header("Configuration")]
    [SerializeField] private bool initializeOnAwake = true;
    [SerializeField] private bool autoRefreshSubscriptions = true;

    private Coroutine subscriptionRefreshCoroutine;

    public bool isPurchasing = false;
    public bool isCheckingSubscriptions = false;

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

        // Set up IAP callbacks and get initial purchases
        IAP.GetViewerPurchases().OnComplete(GetPurchasesCallback);

        isInitialized = true;
        OnInitializeSuccess?.Invoke();

        // Start subscription monitoring if enabled
        if (autoRefreshSubscriptions)
        {
            StartSubscriptionMonitoring();
        }

        // Initial subscription status check
        RefreshSubscriptionStatus();
    }

    /// <summary>
    /// Start automatic subscription status monitoring
    /// </summary>
    private void StartSubscriptionMonitoring()
    {
        if (subscriptionRefreshCoroutine != null)
        {
            StopCoroutine(subscriptionRefreshCoroutine);
        }
        subscriptionRefreshCoroutine = StartCoroutine(SubscriptionMonitoringCoroutine());
    }

    /// <summary>
    /// Coroutine for periodic subscription status refresh
    /// </summary>
    private IEnumerator SubscriptionMonitoringCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(subscriptionRefreshInterval);
            RefreshSubscriptionStatus();
        }
    }

    /// <summary>
    /// Refresh subscription status for all monitored subscriptions
    /// </summary>
    public void RefreshSubscriptionStatus()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Cannot refresh subscription status before initialization");
            return;
        }

        if (subscriptionSKUs == null || subscriptionSKUs.Length == 0)
        {
            Debug.LogWarning("No subscription SKUs configured for monitoring");
            return;
        }

        // Show loading indicator and update status
        ShowSubscriptionLoader(true);
        SetStatusText("Checking subscription status...");

        // Get current purchases to check subscription status
        IAP.GetViewerPurchases().OnComplete(UpdateSubscriptionStatusCallback);
    }

    /// <summary>
    /// Update subscription status based on purchase data
    /// </summary>
    void UpdateSubscriptionStatusCallback(Message<PurchaseList> message)
    {
        try
        {
            if (message.IsError)
            {
                Debug.LogError($"Failed to get subscription status: {message.GetError().Message}");
                SetStatusText("Failed to check subscription status");
                return;
            }

            List<SubscriptionStatus> previousStatuses = new List<SubscriptionStatus>(activeSubscriptions);
            activeSubscriptions.Clear();
            subscriptionDictionary.Clear();

            foreach (Purchase purchase in message.Data)
            {
                // Check if this purchase is for a subscription we're monitoring
                if (Array.Exists(subscriptionSKUs, sku => sku == purchase.Sku))
                {
                    SubscriptionStatus status = CreateSubscriptionStatus(purchase);
                    activeSubscriptions.Add(status);
                    subscriptionDictionary[purchase.Sku] = status;

                    // Check for status changes
                    CheckForSubscriptionStatusChanges(status, previousStatuses);
                }
                else
                {
                    // Handle regular purchases
                    GrantItem(purchase.Sku);
                }
            }

            // Check for expired subscriptions
            CheckForExpiredSubscriptions(previousStatuses);

            OnSubscriptionStatusUpdated?.Invoke(activeSubscriptions);

            // Update status text based on results
            string statusMessage = activeSubscriptions.Count > 0
                ? $"Found {activeSubscriptions.Count} active subscription(s)"
                : "No active subscriptions found";
            SetStatusText(statusMessage);

            Debug.Log($"Updated subscription status. Active subscriptions: {activeSubscriptions.Count}");
        }
        finally
        {
            // Always hide the loader
            ShowSubscriptionLoader(false);
        }
    }

    /// <summary>
    /// Create subscription status from purchase data
    /// </summary>
    private SubscriptionStatus CreateSubscriptionStatus(Purchase purchase)
    {
        SubscriptionStatus status = new SubscriptionStatus
        {
            SKU = purchase.Sku,
            Name = GetProductName(purchase.Sku),
            StartDate = purchase.GrantTime,
            IsActive = true,
            WillRenew = true, // This would need to be determined from Meta's API
            Status = "active"
        };

        // Calculate end date based on subscription period
        // Note: You'll need to implement this based on your subscription periods
        status.EndDate = CalculateSubscriptionEndDate(status.StartDate, purchase.Sku);
        status.IsExpired = DateTime.Now > status.EndDate;
        status.IsActive = !status.IsExpired;

        if (status.IsExpired)
        {
            status.Status = "expired";
        }

        return status;
    }

    /// <summary>
    /// Calculate subscription end date based on the subscription period
    /// </summary>
    private DateTime CalculateSubscriptionEndDate(DateTime startDate, string sku)
    {
        // This is a simplified implementation - you should base this on actual subscription periods
        // You might want to store this information with your products or get it from Meta's API

        // Example logic - adjust based on your subscription types
        if (sku.Contains("monthly"))
        {
            return startDate.AddMonths(1);
        }
        else if (sku.Contains("yearly"))
        {
            return startDate.AddYears(1);
        }
        else if (sku.Contains("weekly"))
        {
            return startDate.AddDays(7);
        }

        // Default to monthly if period can't be determined
        return startDate.AddMonths(1);
    }

    /// <summary>
    /// Check for subscription status changes and fire appropriate events
    /// </summary>
    private void CheckForSubscriptionStatusChanges(SubscriptionStatus currentStatus, List<SubscriptionStatus> previousStatuses)
    {
        SubscriptionStatus previousStatus = previousStatuses.Find(s => s.SKU == currentStatus.SKU);

        if (previousStatus == null)
        {
            // New subscription
            OnSubscriptionActivated?.Invoke(currentStatus);
        }
        else if (previousStatus.IsActive && !currentStatus.IsActive)
        {
            // Subscription expired
            OnSubscriptionExpired?.Invoke(currentStatus);
        }
    }

    /// <summary>
    /// Check for subscriptions that are no longer in the purchase list (cancelled/expired)
    /// </summary>
    private void CheckForExpiredSubscriptions(List<SubscriptionStatus> previousStatuses)
    {
        foreach (SubscriptionStatus previousStatus in previousStatuses)
        {
            if (!subscriptionDictionary.ContainsKey(previousStatus.SKU))
            {
                // Subscription is no longer active
                previousStatus.IsActive = false;
                previousStatus.Status = "expired";
                OnSubscriptionExpired?.Invoke(previousStatus);
            }
        }
    }

    /// <summary>
    /// Get product name for a given SKU
    /// </summary>
    private string GetProductName(string sku)
    {
        if (productDictionary.ContainsKey(sku))
        {
            return productDictionary[sku].Name;
        }
        return sku; // Fallback to SKU if name not found
    }

    /// <summary>
    /// Get current subscription status for a specific SKU
    /// </summary>
    /// <param name="sku">The subscription SKU</param>
    /// <returns>Subscription status or null if not found</returns>
    public SubscriptionStatus GetSubscriptionStatus(string sku)
    {
        return subscriptionDictionary.ContainsKey(sku) ? subscriptionDictionary[sku] : null;
    }

    /// <summary>
    /// Check if user has an active subscription for a specific SKU
    /// </summary>
    /// <param name="sku">The subscription SKU</param>
    /// <returns>True if user has active subscription</returns>
    public bool HasActiveSubscription(string sku)
    {
        SubscriptionStatus status = GetSubscriptionStatus(sku);
        return status != null && status.IsActive;
    }

    /// <summary>
    /// Check if user has any active subscription
    /// </summary>
    /// <returns>True if user has at least one active subscription</returns>
    public bool HasAnyActiveSubscription()
    {
        return activeSubscriptions.Exists(s => s.IsActive);
    }

    /// <summary>
    /// Get all active subscriptions
    /// </summary>
    /// <returns>List of active subscription statuses</returns>
    public List<SubscriptionStatus> GetActiveSubscriptions()
    {
        return activeSubscriptions.FindAll(s => s.IsActive);
    }

    /// <summary>
    /// Fetch available IAP products (including subscriptions)
    /// </summary>
    public void FetchAvailableProducts(string[] skus)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Trying to fetch products before initialization. Queue this request.");
            OnInitializeSuccess += () => FetchAvailableProducts(skus);
            return;
        }

        ShowSubscriptionLoader(true);
        SetStatusText("Loading products...");
        IAP.GetProductsBySKU(skus).OnComplete(GetProductsCallback);
    }

    /// <summary>
    /// Fetch a single product by SKU
    /// </summary>
    public void FetchSingleProduct(string sku)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Trying to fetch product before initialization. Queue this request.");
            OnInitializeSuccess += () => FetchSingleProduct(sku);
            return;
        }

        if (productDictionary.ContainsKey(sku))
        {
            OnSingleProductFetched?.Invoke(productDictionary[sku]);
            return;
        }

        IAP.GetProductsBySKU(new string[] { sku }).OnComplete(GetSingleProductCallback);
    }

    /// <summary>
    /// Callback for retrieving available products
    /// </summary>
    void GetProductsCallback(Message<ProductList> message)
    {
        try
        {
            if (message.IsError)
            {
                Debug.LogError($"Failed to get products: {message.GetError().Message}");
                SetStatusText("Failed to load products");
                return;
            }

            availableProducts.Clear();
            productDictionary.Clear();

            foreach (Product product in message.Data)
            {
                IAPProduct iapProduct = new IAPProduct
                {
                    SKU = product.Sku,
                    Name = product.Name,
                    Description = product.Description,
                    Price = (float)product.Price.AmountInHundredths / 100f,
                    FormattedPrice = product.FormattedPrice,
                    IsSubscription = Array.Exists(subscriptionSKUs, sku => sku == product.Sku)
                };

                // Determine subscription period if it's a subscription
                if (iapProduct.IsSubscription)
                {
                    iapProduct.SubscriptionPeriod = DetermineSubscriptionPeriod(product.Sku);
                }

                availableProducts.Add(iapProduct);
                productDictionary[product.Sku] = iapProduct;
            }

            SetStatusText($"Loaded {availableProducts.Count} products");
            Debug.Log($"Fetched {availableProducts.Count} products");
            OnProductsFetched?.Invoke(availableProducts);
        }
        finally
        {
            ShowSubscriptionLoader(false);
        }
    }

    /// <summary>
    /// Determine subscription period from SKU
    /// </summary>
    private string DetermineSubscriptionPeriod(string sku)
    {
        string lowerSku = sku.ToLower();
        if (lowerSku.Contains("monthly")) return "monthly";
        if (lowerSku.Contains("yearly")) return "yearly";
        if (lowerSku.Contains("weekly")) return "weekly";
        return "unknown";
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
            FormattedPrice = product.FormattedPrice,
            IsSubscription = Array.Exists(subscriptionSKUs, sku => sku == product.Sku)
        };

        if (iapProduct.IsSubscription)
        {
            iapProduct.SubscriptionPeriod = DetermineSubscriptionPeriod(product.Sku);
        }

        productDictionary[product.Sku] = iapProduct;
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
            Debug.Log($"User owns: {purchase.Sku}");

            Debug.Log($"Purchase on: {purchase.GrantTime} \n Expire On:{purchase.ExpirationTime}");

            // Check if it's a subscription
            if (Array.Exists(subscriptionSKUs, sku => sku == purchase.Sku))
            {
                // Handle as subscription - this will be processed in UpdateSubscriptionStatusCallback
                continue;
            }
            else
            {
                // Handle as regular purchase
                GrantItem(purchase.Sku);
            }
        }
    }

    /// <summary>
    /// Initiate a purchase for a product (works for both regular products and subscriptions)
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

        ShowPurchaseLoader(true);
        SetStatusText("Processing purchase...");

        IAP.LaunchCheckoutFlow(sku).OnComplete(ProcessPurchase);
    }

    /// <summary>
    /// Callback when a purchase is completed
    /// </summary>
    void ProcessPurchase(Message<Purchase> message)
    {
        try
        {
            if (message.IsError)
            {
                Debug.LogError($"Purchase failed: {message.GetError().Message}");
                SetStatusText("Purchase failed");
                OnPurchaseFailed?.Invoke(message.GetError().Message);
                currentPurchaseSKU = null;
                return;
            }

            HandleSuccessfulPurchase(message.Data);
        }
        finally
        {
            ShowPurchaseLoader(false);
        }
    }

    /// <summary>
    /// Common handler for successful purchases
    /// </summary>
    private void HandleSuccessfulPurchase(Purchase purchase)
    {
        if (purchase != null)
        {
            Debug.Log($"Successfully purchased {purchase.Sku}");

            SetStatusText("Purchase successful!");

            // Check if it's a subscription
            if (Array.Exists(subscriptionSKUs, sku => sku == purchase.Sku))
            {
                // Refresh subscription status to get the new subscription
                RefreshSubscriptionStatus();
            }
            else
            {
                // Grant regular item
                GrantItem(purchase.Sku);
            }

            OnPurchaseSuccess?.Invoke(purchase.Sku);
            currentPurchaseSKU = null;
        }
    }

    /// <summary>
    /// Grant the purchased item to the user (for non-subscription items)
    /// </summary>
    /// <param name="sku">The SKU of the purchased item</param>
    private void GrantItem(string sku)
    {
        PlayerPrefs.SetInt($"Purchase_{sku}", 1);
        PlayerPrefs.Save();
        Debug.Log($"Item {sku} granted to user");
    }

    /// <summary>
    /// Check if user owns a specific product (for non-subscription items)
    /// </summary>
    /// <param name="sku">The SKU to check</param>
    /// <returns>True if the user owns the product</returns>
    public bool DoesUserOwnProduct(string sku)
    {
        // For subscriptions, check active status instead
        if (Array.Exists(subscriptionSKUs, s => s == sku))
        {
            return HasActiveSubscription(sku);
        }

        return PlayerPrefs.GetInt($"Purchase_{sku}", 0) == 1;
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

        ShowSubscriptionLoader(true);
        SetStatusText("Restoring purchases...");

        IAP.GetViewerPurchases().OnComplete((message) => {
            try
            {
                GetPurchasesCallback(message);
                // Also refresh subscription status
                RefreshSubscriptionStatus();
            }
            finally
            {
                ShowSubscriptionLoader(false);
                SetStatusText("Restore completed");
            }
        });
    }

    /// <summary>
    /// Helper method to show/hide subscription loading indicator
    /// </summary>
    private void ShowSubscriptionLoader(bool show)
    {
        isCheckingSubscriptions = show;

        Debug.Log($"isCheckingSubscriptions - {isCheckingSubscriptions}");

        if (subscriptionLoadingIndicator != null)
            subscriptionLoadingIndicator.SetActive(show);

        if (show)
            OnSubscriptionCheckStarted?.Invoke();
        else
            OnSubscriptionCheckCompleted?.Invoke();
    }

    /// <summary>
    /// Helper method to show/hide purchase loading indicator
    /// </summary>
    private void ShowPurchaseLoader(bool show)
    {
        isPurchasing = show;

        if (purchaseLoadingIndicator != null)
            purchaseLoadingIndicator.SetActive(show);

        if (show)
            OnPurchaseStarted?.Invoke();
        else
            OnPurchaseCompleted?.Invoke();
    }

    /// <summary>
    /// Helper method to set status text safely
    /// </summary>
    private void SetStatusText(string text)
    {
        if (statusText != null)
            statusText.text = text;
    }

    private void OnDestroy()
    {
        if (subscriptionRefreshCoroutine != null)
        {
            StopCoroutine(subscriptionRefreshCoroutine);
        }
    }
}