using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class IAPProduct
{
    public string SKU;
    public string Name;
    public string Description;
    public float Price;
    public string FormattedPrice;
    public bool IsSubscription;
    public string SubscriptionPeriod;
}

[System.Serializable]
public class SubscriptionStatus
{
    public string SKU;
    public string Name;
    public bool IsActive;
    public DateTime StartDate;
    public DateTime EndDate;
    public DateTime GrantTime;      // When the subscription was purchased
    public DateTime ExpirationTime; // When it actually expires (from Meta API)
    public bool IsExpired;
    public bool WillRenew;
    public string Status;
    public bool IsInGracePeriod;    // For handling payment failures
    public TimeSpan GracePeriod = TimeSpan.FromDays(3); // 3-day grace period

    public int DaysRemaining => IsActive ? Mathf.Max(0, (int)(EndDate - DateTime.Now).TotalDays) : 0;
    public int HoursRemaining => IsActive ? Mathf.Max(0, (int)(EndDate - DateTime.Now).TotalHours) : 0;
    public bool IsExpiringSoon => DaysRemaining <= 7 && IsActive;
    public bool IsExpiringToday => DaysRemaining == 0 && IsActive;

    // Check if subscription is valid right now
    public bool IsCurrentlyValid => DateTime.Now < EndDate && IsActive;

    // Get remaining time as formatted string
    public string GetRemainingTimeString()
    {
        if (!IsActive) return "Expired";

        var remaining = EndDate - DateTime.Now;
        if (remaining.TotalDays >= 1)
            return $"{(int)remaining.TotalDays} days";
        else if (remaining.TotalHours >= 1)
            return $"{(int)remaining.TotalHours} hours";
        else if (remaining.TotalMinutes >= 1)
            return $"{(int)remaining.TotalMinutes} minutes";
        else
            return "Expires soon";
    }
}

[Serializable]
public class IAPButton
{
    public string SKU;
    public Button button;
    public bool isOwned;
    public bool isSubscription;
}

public class MetaIAPManager : MonoBehaviour
{
    public static MetaIAPManager Instance { get; private set; }

    public List<IAPProduct> availableProducts = new List<IAPProduct>();
    public List<SubscriptionStatus> activeSubscriptions = new List<SubscriptionStatus>();
    public List<IAPButton> registeredButtons = new List<IAPButton>();

    private Dictionary<string, IAPProduct> productDictionary = new Dictionary<string, IAPProduct>();
    private Dictionary<string, SubscriptionStatus> subscriptionDictionary = new Dictionary<string, SubscriptionStatus>();
    private Dictionary<string, IAPButton> buttonDictionary = new Dictionary<string, IAPButton>();
    private string currentPurchaseSKU;
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
    public float subscriptionRefreshInterval = 300f;
    [Tooltip("Enable debug logging for subscription validation")]
    public bool enableSubscriptionDebugLog = true;

    // Enhanced Events
    public event Action<List<IAPProduct>> OnProductsFetched;
    public event Action<IAPProduct> OnSingleProductFetched;
    public event Action<string> OnPurchaseSuccess;
    public event Action<string> OnPurchaseFailed;
    public event Action<string> OnInitializeFailed;
    public event Action OnInitializeSuccess;
    public event Action<List<SubscriptionStatus>> OnSubscriptionStatusUpdated;
    public event Action<SubscriptionStatus> OnSubscriptionActivated;
    public event Action<SubscriptionStatus> OnSubscriptionExpired;
    public event Action<SubscriptionStatus> OnSubscriptionExpiringSoon;
    public event Action<SubscriptionStatus> OnSubscriptionRenewed;
    public event Action<SubscriptionStatus> OnSubscriptionCancelled;
    public event Action<string, bool> OnProductOwnershipChecked;
    public event Action<List<SubscriptionStatus>> OnUserHasActiveSubscriptions;

    [Header("Configuration")]
    [SerializeField] private bool initializeOnAwake = true;
    [SerializeField] private bool autoRefreshSubscriptions = true;

    private Coroutine subscriptionRefreshCoroutine;
    private Coroutine subscriptionValidationCoroutine;
    public bool isPurchasing = false;
    public bool isCheckingSubscriptions = false;
    public bool isCheckingOwnership = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (initializeOnAwake)
            {
                Debug.LogWarning("-----------------------");
                Initialize();
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void GetAllIAPButton()
    {
        SimplifiedMetaIAP[] components = FindObjectsOfType<SimplifiedMetaIAP>(true);
        foreach (var component in components)
        {
            component.InitializeButton();
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeButtonsAfterDelay());

        // Start continuous subscription validation
        //if (subscriptionValidationCoroutine == null)
        //{
        //    subscriptionValidationCoroutine = StartCoroutine(ContinuousSubscriptionValidation());
        //}
    }

    private IEnumerator InitializeButtonsAfterDelay()
    {
        GetAllIAPButton();
        yield return new WaitForSeconds(0.1f);

        if (isInitialized)
        {
            CheckAllRegisteredProducts();
        }
        else
        {
            OnInitializeSuccess += CheckAllRegisteredProducts;
        }
    }

    // Continuous validation to catch expiring subscriptions
    private IEnumerator ContinuousSubscriptionValidation()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f); // Check every minute
            ValidateActiveSubscriptions();
        }
    }

    private void ValidateActiveSubscriptions()
    {
        if (activeSubscriptions.Count == 0) return;

        List<SubscriptionStatus> expiredSubscriptions = new List<SubscriptionStatus>();
        List<SubscriptionStatus> expiringSoonSubscriptions = new List<SubscriptionStatus>();

        foreach (var subscription in activeSubscriptions)
        {
            // Check if subscription has expired
            if (DateTime.Now >= subscription.EndDate && subscription.IsActive)
            {
                subscription.IsActive = false;
                subscription.IsExpired = true;
                subscription.Status = "expired";
                expiredSubscriptions.Add(subscription);

                if (enableSubscriptionDebugLog)
                {
                    Debug.LogWarning($"Subscription {subscription.SKU} has expired!");
                }
            }
            // Check if subscription is expiring soon
            else if (subscription.IsExpiringSoon && subscription.IsActive)
            {
                expiringSoonSubscriptions.Add(subscription);

                if (enableSubscriptionDebugLog)
                {
                    Debug.LogWarning($"Subscription {subscription.SKU} expires in {subscription.DaysRemaining} days!");
                }
            }
        }

        // Trigger events for expired subscriptions
        foreach (var expired in expiredSubscriptions)
        {
            OnSubscriptionExpired?.Invoke(expired);
            UpdateButtonState(expired.SKU, false); // Re-enable purchase button
        }

        // Trigger events for expiring soon subscriptions
        foreach (var expiring in expiringSoonSubscriptions)
        {
            OnSubscriptionExpiringSoon?.Invoke(expiring);
        }

        // Update the subscription dictionary
        foreach (var subscription in activeSubscriptions)
        {
            subscriptionDictionary[subscription.SKU] = subscription;
        }
    }

    public void RegisterButton(string sku, Button button)
    {
        if (buttonDictionary.ContainsKey(sku))
        {
            buttonDictionary[sku].button = button;
            return;
        }

        IAPButton iapButton = new IAPButton
        {
            SKU = sku,
            button = button,
            isOwned = false,
            isSubscription = Array.Exists(subscriptionSKUs, s => s == sku)
        };

        registeredButtons.Add(iapButton);
        buttonDictionary[sku] = iapButton;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => PurchaseProduct(sku));
            button.interactable = false;
        }

        Debug.Log($"Registered button for SKU: {sku}");
    }

    private void CheckAllRegisteredProducts()
    {
        if (registeredButtons.Count == 0)
        {
            Debug.Log("No buttons registered for ownership check");
            return;
        }

        ShowSubscriptionLoader(true);
        SetStatusText("Checking product ownership...");

        foreach (IAPButton iapButton in registeredButtons)
        {
            CheckSingleProductOwnership(iapButton.SKU);
        }
    }

    private void CheckSingleProductOwnership(string sku)
    {
        if (!isInitialized)
        {
            return;
        }

        IAP.GetViewerPurchases().OnComplete((message) => CheckOwnershipCallback(message, sku));
    }

    void CheckOwnershipCallback(Message<PurchaseList> message, string targetSku)
    {
        bool ownsProduct = false;

        if (!message.IsError)
        {
            foreach (Purchase purchase in message.Data)
            {
                if (purchase.Sku == targetSku)
                {
                    if (Array.Exists(subscriptionSKUs, sku => sku == targetSku))
                    {
                        SubscriptionStatus status = CreateSubscriptionStatusFromPurchase(purchase);
                        ownsProduct = status.IsCurrentlyValid; // Use the new validation method
                        if (ownsProduct && !subscriptionDictionary.ContainsKey(targetSku))
                        {
                            activeSubscriptions.Add(status);
                            subscriptionDictionary[targetSku] = status;
                            LogSubscriptionDetails(status);
                        }
                    }
                    else
                    {
                        ownsProduct = true;
                        GrantItem(targetSku);
                    }
                    break;
                }
            }
        }

        if (!ownsProduct && !message.IsError)
        {
            ownsProduct = PlayerPrefs.GetInt($"Purchase_{targetSku}", 0) == 1;
        }

        UpdateButtonState(targetSku, ownsProduct);
        OnProductOwnershipChecked?.Invoke(targetSku, ownsProduct);

        CheckIfAllProductsChecked();
    }

    private void CheckIfAllProductsChecked()
    {
        bool allChecked = true;
        foreach (IAPButton iapButton in registeredButtons)
        {
            if (!iapButton.isOwned && DoesUserOwnProduct(iapButton.SKU))
            {
                allChecked = false;
                break;
            }
        }

        if (allChecked)
        {
            ShowSubscriptionLoader(false);
            SetStatusText("Product check completed");

            var validSubscriptions = GetActiveSubscriptions();
            if (validSubscriptions.Count > 0)
            {
                OnUserHasActiveSubscriptions?.Invoke(validSubscriptions);
                Debug.Log($"User has {validSubscriptions.Count} active subscription(s)");
            }
        }
    }

    private void UpdateButtonState(string sku, bool isOwned)
    {
        if (buttonDictionary.ContainsKey(sku))
        {
            IAPButton iapButton = buttonDictionary[sku];
            iapButton.isOwned = isOwned;

            if (iapButton.button != null)
            {
                iapButton.button.interactable = !isOwned;
            }

            Debug.Log($"Button for {sku} - Owned: {isOwned}, Interactable: {!isOwned}");
        }
    }

    private void LogSubscriptionDetails(SubscriptionStatus subscription)
    {
        if (!enableSubscriptionDebugLog) return;

        Debug.Log($"<color=green>SUBSCRIPTION DETAILS:</color>");
        Debug.Log($"<color=yellow>SKU:</color> {subscription.SKU}");
        Debug.Log($"<color=yellow>Name:</color> {subscription.Name}");
        Debug.Log($"<color=yellow>Status:</color> {subscription.Status}");
        Debug.Log($"<color=yellow>Grant Time:</color> {subscription.GrantTime:yyyy-MM-dd HH:mm:ss}");
        Debug.Log($"<color=yellow>Start Date:</color> {subscription.StartDate:yyyy-MM-dd HH:mm:ss}");
        Debug.Log($"<color=yellow>End Date:</color> {subscription.EndDate:yyyy-MM-dd HH:mm:ss}");
        Debug.Log($"<color=yellow>Expiration Time (API):</color> {subscription.ExpirationTime:yyyy-MM-dd HH:mm:ss}");
        Debug.Log($"<color=yellow>Days Remaining:</color> {subscription.DaysRemaining}");
        Debug.Log($"<color=yellow>Hours Remaining:</color> {subscription.HoursRemaining}");
        Debug.Log($"<color=yellow>Remaining Time:</color> {subscription.GetRemainingTimeString()}");
        Debug.Log($"<color=yellow>Is Active:</color> {subscription.IsActive}");
        Debug.Log($"<color=yellow>Is Currently Valid:</color> {subscription.IsCurrentlyValid}");
        Debug.Log($"<color=yellow>Will Renew:</color> {subscription.WillRenew}");
        Debug.Log($"<color=yellow>Is Expiring Soon:</color> {subscription.IsExpiringSoon}");
        Debug.Log($"<color=yellow>Is Expiring Today:</color> {subscription.IsExpiringToday}");
    }

    public void Initialize()
    {
        try
        {
            Core.Initialize();
            Debug.Log("Meta XR Platform SDK initialized successfully");
            Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Meta XR Platform SDK: {e.Message}");
            OnInitializeFailed?.Invoke(e.Message);
        }
    }

    void EntitlementCallback(Message message)
    {
        if (message.IsError)
        {
            Debug.LogError($"Entitlement check failed: {message.GetError().Message}");
            OnInitializeFailed?.Invoke(message.GetError().Message);
            return;
        }

        Debug.Log("User is entitled to use this application");
        IAP.GetViewerPurchases().OnComplete(GetPurchasesCallback);

        isInitialized = true;
        OnInitializeSuccess?.Invoke();

        if (autoRefreshSubscriptions)
        {
            StartSubscriptionMonitoring();
        }

        RefreshSubscriptionStatus();
    }

    private void StartSubscriptionMonitoring()
    {
        if (subscriptionRefreshCoroutine != null)
        {
            StopCoroutine(subscriptionRefreshCoroutine);
        }
        subscriptionRefreshCoroutine = StartCoroutine(SubscriptionMonitoringCoroutine());
    }

    private IEnumerator SubscriptionMonitoringCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(subscriptionRefreshInterval);
            RefreshSubscriptionStatus();
        }
    }

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

        ShowSubscriptionLoader(true);
        SetStatusText("Checking subscription status...");

        IAP.GetViewerPurchases().OnComplete(UpdateSubscriptionStatusCallback);
    }

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
                if (Array.Exists(subscriptionSKUs, sku => sku == purchase.Sku))
                {
                    SubscriptionStatus status = CreateSubscriptionStatusFromPurchase(purchase);

                    // Only add if the subscription is currently valid
                    if (status.IsCurrentlyValid)
                    {
                        activeSubscriptions.Add(status);
                        subscriptionDictionary[purchase.Sku] = status;
                        LogSubscriptionDetails(status);
                        CheckForSubscriptionStatusChanges(status, previousStatuses);
                    }
                    else
                    {
                        Debug.LogWarning($"Subscription {purchase.Sku} is expired and will not be added to active list");
                        // Still check for status changes to trigger expiration events
                        CheckForSubscriptionStatusChanges(status, previousStatuses);
                    }
                }
                else
                {
                    GrantItem(purchase.Sku);
                }
            }

            CheckForExpiredSubscriptions(previousStatuses);
            OnSubscriptionStatusUpdated?.Invoke(activeSubscriptions);

            var validSubscriptions = GetActiveSubscriptions();
            if (validSubscriptions.Count > 0)
            {
                OnUserHasActiveSubscriptions?.Invoke(validSubscriptions);
            }

            string statusMessage = validSubscriptions.Count > 0
                ? $"Found {validSubscriptions.Count} active subscription(s)"
                : "No active subscriptions found";
            SetStatusText(statusMessage);

            Debug.Log($"Updated subscription status. Active subscriptions: {validSubscriptions.Count}");
        }
        finally
        {
            ShowSubscriptionLoader(false);
        }
    }

    private SubscriptionStatus CreateSubscriptionStatusFromPurchase(Purchase purchase)
    {
        SubscriptionStatus status = new SubscriptionStatus
        {
            SKU = purchase.Sku,
            Name = GetProductName(purchase.Sku),
            GrantTime = purchase.GrantTime,
            StartDate = purchase.GrantTime,
            ExpirationTime = purchase.ExpirationTime,
            WillRenew = true,
            Status = "active"
        };

        // Use Meta's expiration time if available, otherwise calculate
        if (purchase.ExpirationTime != DateTime.MinValue && purchase.ExpirationTime > DateTime.Now)
        {
            status.EndDate = purchase.ExpirationTime;
        }
        else
        {
            // Fallback to calculation if API doesn't provide expiration time
            status.EndDate = CalculateSubscriptionEndDate(status.StartDate, purchase.Sku);
        }

        // Validate current status
        DateTime now = DateTime.Now;
        status.IsExpired = now >= status.EndDate;
        status.IsActive = !status.IsExpired;

        if (status.IsExpired)
        {
            status.Status = "expired";
            status.WillRenew = false;
        }

        // Check for grace period (for payment failures)
        if (status.IsExpired && (now - status.EndDate) <= status.GracePeriod)
        {
            status.IsInGracePeriod = true;
            status.Status = "grace_period";
        }

        return status;
    }

    private DateTime CalculateSubscriptionEndDate(DateTime startDate, string sku)
    {
        string lowerSku = sku.ToLower();

        if (lowerSku.Contains("monthly") || lowerSku.Contains("month"))
        {
            return startDate.AddMonths(1);
        }
        else if (lowerSku.Contains("yearly") || lowerSku.Contains("year"))
        {
            return startDate.AddYears(1);
        }
        else if (lowerSku.Contains("weekly") || lowerSku.Contains("week"))
        {
            return startDate.AddDays(7);
        }
        else if (lowerSku.Contains("daily") || lowerSku.Contains("day"))
        {
            return startDate.AddDays(1);
        }
        else if (lowerSku.Contains("quarterly") || lowerSku.Contains("quarter"))
        {
            return startDate.AddMonths(3);
        }

        // Default to monthly if we can't determine the period
        Debug.LogWarning($"Could not determine subscription period for {sku}, defaulting to monthly");
        return startDate.AddMonths(1);
    }

    private void CheckForSubscriptionStatusChanges(SubscriptionStatus currentStatus, List<SubscriptionStatus> previousStatuses)
    {
        SubscriptionStatus previousStatus = previousStatuses.Find(s => s.SKU == currentStatus.SKU);

        if (previousStatus == null && currentStatus.IsActive)
        {
            // New subscription activated
            OnSubscriptionActivated?.Invoke(currentStatus);
        }
        else if (previousStatus != null)
        {
            // Check for renewal (new grant time with same SKU)
            if (currentStatus.GrantTime > previousStatus.GrantTime)
            {
                OnSubscriptionRenewed?.Invoke(currentStatus);
            }

            // Check for expiration
            if (previousStatus.IsActive && !currentStatus.IsActive)
            {
                OnSubscriptionExpired?.Invoke(currentStatus);
            }
        }
    }

    private void CheckForExpiredSubscriptions(List<SubscriptionStatus> previousStatuses)
    {
        foreach (SubscriptionStatus previousStatus in previousStatuses)
        {
            if (!subscriptionDictionary.ContainsKey(previousStatus.SKU) && previousStatus.IsActive)
            {
                // Subscription was active but is no longer in the list - it expired
                previousStatus.IsActive = false;
                previousStatus.IsExpired = true;
                previousStatus.Status = "expired";
                OnSubscriptionExpired?.Invoke(previousStatus);
            }
        }
    }

    private string GetProductName(string sku)
    {
        if (productDictionary.ContainsKey(sku))
        {
            return productDictionary[sku].Name;
        }
        return sku;
    }

    // Enhanced subscription query methods
    public SubscriptionStatus GetSubscriptionStatus(string sku)
    {
        return subscriptionDictionary.ContainsKey(sku) ? subscriptionDictionary[sku] : null;
    }

    public bool HasActiveSubscription(string sku)
    {
        SubscriptionStatus status = GetSubscriptionStatus(sku);
        return status != null && status.IsCurrentlyValid;
    }

    public bool HasAnyActiveSubscription()
    {
        return activeSubscriptions.Exists(s => s.IsCurrentlyValid);
    }

    public List<SubscriptionStatus> GetActiveSubscriptions()
    {
        return activeSubscriptions.FindAll(s => s.IsCurrentlyValid);
    }

    public List<SubscriptionStatus> GetExpiringSoonSubscriptions()
    {
        return activeSubscriptions.FindAll(s => s.IsExpiringSoon);
    }

    public SubscriptionStatus GetMostRecentSubscription()
    {
        if (activeSubscriptions.Count == 0) return null;

        SubscriptionStatus mostRecent = activeSubscriptions[0];
        foreach (var subscription in activeSubscriptions)
        {
            if (subscription.GrantTime > mostRecent.GrantTime)
            {
                mostRecent = subscription;
            }
        }
        return mostRecent;
    }

    // Rest of the methods remain the same...
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

    private string DetermineSubscriptionPeriod(string sku)
    {
        string lowerSku = sku.ToLower();
        if (lowerSku.Contains("monthly") || lowerSku.Contains("month")) return "monthly";
        if (lowerSku.Contains("yearly") || lowerSku.Contains("year")) return "yearly";
        if (lowerSku.Contains("weekly") || lowerSku.Contains("week")) return "weekly";
        if (lowerSku.Contains("daily") || lowerSku.Contains("day")) return "daily";
        if (lowerSku.Contains("quarterly") || lowerSku.Contains("quarter")) return "quarterly";
        return "unknown";
    }

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

    void GetPurchasesCallback(Message<PurchaseList> message)
    {
        if (message.IsError)
        {
            Debug.LogError($"Failed to get purchases: {message.GetError().Message}");
            return;
        }

        ShowSubscriptionLoader(true);

        Debug.Log($"User has {message.Data.Count} previous purchases");

        bool hasValidSubscriptions = false;
        foreach (Purchase purchase in message.Data)
        {
            Debug.Log($"User owns: {purchase.Sku}");
            Debug.Log($"Purchase on: {purchase.GrantTime} \n Expire On:{purchase.ExpirationTime}");

            if (Array.Exists(subscriptionSKUs, sku => sku == purchase.Sku))
            {
                var status = CreateSubscriptionStatusFromPurchase(purchase);
                if (status.IsCurrentlyValid)
                {
                    AccountPage.ap.SetUpSubscriptionText(status.EndDate.ToString("dd-mm-yyyy"));
                    hasValidSubscriptions = true;
                }
            }
            else
            {
                GrantItem(purchase.Sku);
            }
        }

        // Update account page with actual subscription status
        if (AccountPage.ap != null)
        {
            AccountPage.ap.SetSubscriptionStatus(hasValidSubscriptions);
            ShowSubscriptionLoader(false);
        }
    }

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

    private void HandleSuccessfulPurchase(Purchase purchase)
    {
        if (purchase != null)
        {
            Debug.Log($"Successfully purchased {purchase.Sku}");
            SetStatusText("Purchase successful!");

            if (Array.Exists(subscriptionSKUs, sku => sku == purchase.Sku))
            {
                SubscriptionStatus newSubscription = CreateSubscriptionStatusFromPurchase(purchase);

                // Only add if the subscription is valid
                if (newSubscription.IsCurrentlyValid)
                {
                    activeSubscriptions.Add(newSubscription);
                    subscriptionDictionary[purchase.Sku] = newSubscription;
                    LogSubscriptionDetails(newSubscription);
                    OnSubscriptionActivated?.Invoke(newSubscription);
                    OnUserHasActiveSubscriptions?.Invoke(GetActiveSubscriptions());
                }
                else
                {
                    Debug.LogError($"Purchased subscription {purchase.Sku} is already expired!");
                }
            }
            else
            {
                GrantItem(purchase.Sku);
            }

            UpdateButtonState(purchase.Sku, true);
            OnPurchaseSuccess?.Invoke(purchase.Sku);
            currentPurchaseSKU = null;
        }
    }

    private void GrantItem(string sku)
    {
        PlayerPrefs.SetInt($"Purchase_{sku}", 1);
        PlayerPrefs.Save();
        Debug.Log($"Item {sku} granted to user");
    }

    public bool DoesUserOwnProduct(string sku)
    {
        if (Array.Exists(subscriptionSKUs, s => s == sku))
        {
            return HasActiveSubscription(sku);
        }

        return PlayerPrefs.GetInt($"Purchase_{sku}", 0) == 1;
    }

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
                RefreshSubscriptionStatus();
            }
            finally
            {
                ShowSubscriptionLoader(false);
                SetStatusText("Restore completed");
            }
        });
    }

    private void ShowSubscriptionLoader(bool show)
    {
        isCheckingSubscriptions = show;

        Debug.Log($"isCheckingSubscriptions - {isCheckingSubscriptions}");

        if (subscriptionLoadingIndicator != null)
            subscriptionLoadingIndicator.SetActive(show);
    }

    private void ShowPurchaseLoader(bool show)
    {
        isPurchasing = show;

        if (purchaseLoadingIndicator != null)
            purchaseLoadingIndicator.SetActive(show);
    }

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

        if (subscriptionValidationCoroutine != null)
        {
            StopCoroutine(subscriptionValidationCoroutine);
        }
    }

    // Additional utility methods for subscription management

    /// <summary>
    /// Get detailed subscription information for UI display
    /// </summary>
    public string GetSubscriptionDisplayInfo(string sku)
    {
        var subscription = GetSubscriptionStatus(sku);
        if (subscription == null) return "No subscription found";

        if (!subscription.IsCurrentlyValid) return "Subscription expired";

        return $"Active until {subscription.EndDate:MMM dd, yyyy} ({subscription.GetRemainingTimeString()} remaining)";
    }

    /// <summary>
    /// Check if user should be prompted to renew subscription
    /// </summary>
    public bool ShouldPromptRenewal(string sku)
    {
        var subscription = GetSubscriptionStatus(sku);
        return subscription != null && subscription.IsExpiringSoon;
    }

    /// <summary>
    /// Get the next renewal date for a subscription
    /// </summary>
    public DateTime? GetNextRenewalDate(string sku)
    {
        var subscription = GetSubscriptionStatus(sku);
        if (subscription == null || !subscription.WillRenew) return null;

        return subscription.EndDate;
    }

    /// <summary>
    /// Force refresh a specific subscription status
    /// </summary>
    public void RefreshSpecificSubscription(string sku, Action<SubscriptionStatus> callback = null)
    {
        if (!isInitialized) return;

        IAP.GetViewerPurchases().OnComplete((message) => {
            if (message.IsError)
            {
                callback?.Invoke(null);
                return;
            }

            foreach (Purchase purchase in message.Data)
            {
                if (purchase.Sku == sku)
                {
                    var status = CreateSubscriptionStatusFromPurchase(purchase);

                    if (status.IsCurrentlyValid)
                    {
                        subscriptionDictionary[sku] = status;

                        // Update the active subscriptions list
                        var existingIndex = activeSubscriptions.FindIndex(s => s.SKU == sku);
                        if (existingIndex >= 0)
                        {
                            activeSubscriptions[existingIndex] = status;
                        }
                        else
                        {
                            activeSubscriptions.Add(status);
                        }
                    }
                    else
                    {
                        // Remove from active subscriptions if expired
                        subscriptionDictionary.Remove(sku);
                        activeSubscriptions.RemoveAll(s => s.SKU == sku);
                    }

                    callback?.Invoke(status);
                    return;
                }
            }

            // Subscription not found in purchases
            callback?.Invoke(null);
        });
    }
}