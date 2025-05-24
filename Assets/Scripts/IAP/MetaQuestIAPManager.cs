using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;

public class MetaQuestIAPManager : MonoBehaviour
{
    [System.Serializable]
    public class ProductInfo
    {
        public string sku;
        public string name;
        public string description;
        public string formattedPrice;
        public string currency;
        public int priceAmountInCents;
        public ProductType productType;
    }

    [Header("Product Configuration")]
    [SerializeField] private List<string> productSKUs = new List<string>();
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    // Events
    public System.Action<List<ProductInfo>> OnProductsLoaded;
    public System.Action<string> OnProductLoadError;
    
    // Storage for retrieved products
    private List<ProductInfo> availableProducts = new List<ProductInfo>();

    bool isInitialized = false;
    
    void Start()
    {
        InitializeOculusPlatform();
    }
    
    private void InitializeOculusPlatform()
    {
        try
        {
            if (!isInitialized)
            {
                Core.AsyncInitialize().OnComplete(OnOculusPlatformInitialized);
            }
            else
            {
                LogDebug("Oculus Platform already initialized");
                LoadProductDetails();
            }
        }
        catch (System.Exception e)
        {
            LogError($"Failed to initialize Oculus Platform: {e.Message}");
            OnProductLoadError?.Invoke($"Platform initialization failed: {e.Message}");
        }
    }
    
    private void OnOculusPlatformInitialized(Message msg)
    {
        if (msg.IsError)
        {
            LogError($"Oculus Platform initialization failed: {msg.GetError().Message}");
            OnProductLoadError?.Invoke($"Platform initialization failed: {msg.GetError().Message}");
            return;
        }

        isInitialized = true;


        LogDebug("Oculus Platform initialized successfully");
        LoadProductDetails();
    }
    
    public void LoadProductDetails()
    {
        if (productSKUs == null || productSKUs.Count == 0)
        {
            LogError("No product SKUs configured. Please add SKUs to the productSKUs list.");
            OnProductLoadError?.Invoke("No product SKUs configured");
            return;
        }
        
        LogDebug($"Loading details for {productSKUs.Count} products...");
        
        // Convert List<string> to string array
        string[] skuArray = productSKUs.ToArray();
        
        // Request product details from Oculus Platform
        IAP.GetProductsBySKU(skuArray).OnComplete(OnProductDetailsReceived);
    }
    
    private void OnProductDetailsReceived(Message<ProductList> message)
    {
        if (message.IsError)
        {
            string errorMsg = $"Failed to load product details: {message.GetError().Message}";
            LogError(errorMsg);
            OnProductLoadError?.Invoke(errorMsg);
            return;
        }
        
        availableProducts.Clear();
        ProductList productList = message.Data;
        
        LogDebug($"Successfully loaded {productList.Count} products");
        
        foreach (Product product in productList)
        {
            ProductInfo productInfo = new ProductInfo
            {
                sku = product.Sku,
                name = product.Name,
                description = product.Description,
                formattedPrice = product.FormattedPrice,
                productType = product.Type
            };
            
            availableProducts.Add(productInfo);
            
            LogDebug($"Product: {productInfo.name} | SKU: {productInfo.sku} | Price: {productInfo.formattedPrice}");
        }
        
        // Notify listeners that products have been loaded
        OnProductsLoaded?.Invoke(availableProducts);
    }
    
    // Public methods to access product information
    public List<ProductInfo> GetAllProducts()
    {
        return new List<ProductInfo>(availableProducts);
    }
    
    public ProductInfo GetProductBySKU(string sku)
    {
        return availableProducts.Find(p => p.sku == sku);
    }
    
    public List<ProductInfo> GetProductsByType(ProductType type)
    {
        return availableProducts.FindAll(p => p.productType == type);
    }
    
    public bool IsProductAvailable(string sku)
    {
        return availableProducts.Exists(p => p.sku == sku);
    }
    
    // Method to add SKUs programmatically
    public void AddProductSKU(string sku)
    {
        if (!productSKUs.Contains(sku))
        {
            productSKUs.Add(sku);
            LogDebug($"Added SKU: {sku}");
        }
    }
    
    public void AddProductSKUs(List<string> skus)
    {
        foreach (string sku in skus)
        {
            AddProductSKU(sku);
        }
    }
    
    // Method to refresh product data
    public void RefreshProductData()
    {
        LogDebug("Refreshing product data...");
        LoadProductDetails();
    }
    
    // Debug helper methods
    public void PrintAllProducts()
    {
        if (availableProducts.Count == 0)
        {
            LogDebug("No products loaded");
            return;
        }
        
        LogDebug("=== Available Products ===");
        foreach (ProductInfo product in availableProducts)
        {
            LogDebug($"SKU: {product.sku}");
            LogDebug($"Name: {product.name}");
            LogDebug($"Description: {product.description}");
            LogDebug($"Price: {product.formattedPrice} ({product.currency})");
            LogDebug($"Type: {product.productType}");
            LogDebug("---");
        }
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[MetaQuestIAP] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[MetaQuestIAP] {message}");
    }
    
    // Example usage method
    [ContextMenu("Load Example Products")]
    public void LoadExampleProducts()
    {
        // Add some example SKUs - replace these with your actual product SKUs
        productSKUs.Clear();
        productSKUs.AddRange(new List<string>
        {
            "com.yourcompany.yourapp.premium_upgrade",
            "com.yourcompany.yourapp.coins_pack_small",
            "com.yourcompany.yourapp.coins_pack_large",
            "com.yourcompany.yourapp.remove_ads"
        });
        
        LoadProductDetails();
    }
}