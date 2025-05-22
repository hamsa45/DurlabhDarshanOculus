using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A simplified IAP manager for Meta Quest that handles single product purchases
/// with a direct purchase button and loading indicator.
/// </summary>
public class SimplifiedMetaIAP : MonoBehaviour
{
    [Header("Product Configuration")]
    [Tooltip("The SKU of the product to purchase")]
    public string productSKU = "com.yourcompany.yourgame.item1";
    [Tooltip("The button to trigger the purchase")]
    public Button purchaseButton;
    
    private bool isProcessing = false;

    void Start()
    {
        // Subscribe to IAP events
        if (MetaIAPManager.Instance != null)
        {
            MetaIAPManager.Instance.OnPurchaseSuccess += OnPurchaseSuccess;
            MetaIAPManager.Instance.OnPurchaseFailed += OnPurchaseFailed;
            MetaIAPManager.Instance.OnInitializeFailed += OnInitFailed;
        }
        else
        {
            Debug.LogError("MetaIAPManager instance not found!");
            if (MetaIAPManager.Instance.statusText != null)
                MetaIAPManager.Instance.statusText.text = "IAP system not available";
            DisablePurchaseButton();
        }
        
        // Set up button click event
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(InitiatePurchase);
            
            // Check if product is already owned
            StartCoroutine(CheckOwnershipAfterDelay());
        }
        
        // Initially hide loading indicator
        if (MetaIAPManager.Instance.loadingIndicator != null)
            MetaIAPManager.Instance.loadingIndicator.SetActive(false);
    }
    
    IEnumerator CheckOwnershipAfterDelay()
    {
        // Wait for IAP system to initialize
        yield return new WaitForSeconds(1.5f);
        
        if (MetaIAPManager.Instance != null && 
            MetaIAPManager.Instance.DoesUserOwnProduct(productSKU))
        {
            // Product already owned
            DisablePurchaseButton();
            if (MetaIAPManager.Instance.statusText != null)
                MetaIAPManager.Instance.statusText.text = "Already purchased";
        }
        else
        {
            // Product available for purchase
            EnablePurchaseButton();
            if (MetaIAPManager.Instance.statusText != null)
                MetaIAPManager.Instance.statusText.text = "Ready to purchase";
        }
    }
    
    public void InitiatePurchase()
    {
        if (isProcessing || MetaIAPManager.Instance == null)
            return;
            
        isProcessing = true;
        
        // Show loading indicator
        if (MetaIAPManager.Instance.loadingIndicator != null)
            MetaIAPManager.Instance.loadingIndicator.SetActive(true);
            
        // Update status
        if (MetaIAPManager.Instance.statusText != null)
            MetaIAPManager.Instance.statusText.text = "Processing purchase...";
            
        // Disable purchase button during processing
        DisablePurchaseButton();
        
        // Initiate purchase
        MetaIAPManager.Instance.PurchaseProduct(productSKU);
    }
    
    void OnPurchaseSuccess(string sku)
    {
        if (sku != productSKU)
            return;
            
        isProcessing = false;
        
        // Hide loading indicator
        if (MetaIAPManager.Instance.loadingIndicator != null)
            MetaIAPManager.Instance.loadingIndicator.SetActive(false);
            
        // Update status
        if (MetaIAPManager.Instance.statusText != null)
            MetaIAPManager.Instance.statusText.text = "Purchase complete!";
            
        // Keep button disabled as product is now owned
        DisablePurchaseButton();
        
        // Apply purchase effects in your game
        ApplyPurchaseEffect();
    }
    
    void OnPurchaseFailed(string errorMsg)
    {
        isProcessing = false;
        
        // Hide loading indicator
        if (MetaIAPManager.Instance.loadingIndicator != null)
            MetaIAPManager.Instance.loadingIndicator.SetActive(false);
            
        // Update status
        if (MetaIAPManager.Instance.statusText != null)
            MetaIAPManager.Instance.statusText.text = "Purchase failed";
            
        // Re-enable purchase button
        EnablePurchaseButton();
        
        Debug.LogError($"Purchase failed: {errorMsg}");
    }
    
    void OnInitFailed(string errorMsg)
    {
        isProcessing = false;
        
        // Hide loading indicator
        if (MetaIAPManager.Instance.loadingIndicator != null)
            MetaIAPManager.Instance.loadingIndicator.SetActive(false);
            
        // Update status
        if (MetaIAPManager.Instance.statusText != null)
            MetaIAPManager.Instance.statusText.text = "IAP not available";
            
        // Disable purchase button
        DisablePurchaseButton();
        
        Debug.LogError($"IAP initialization failed: {errorMsg}");
    }
    
    void ApplyPurchaseEffect()
    {
        // Implement the effect of the purchase here
        // For example:
        // - Remove ads
        // - Add coins
        // - Unlock features
        Debug.Log($"Purchase effect applied for {productSKU}");
    }
    
    private void EnablePurchaseButton()
    {
        if (purchaseButton != null)
            purchaseButton.interactable = true;
    }
    
    private void DisablePurchaseButton()
    {
        if (purchaseButton != null)
            purchaseButton.interactable = false;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (MetaIAPManager.Instance != null)
        {
            MetaIAPManager.Instance.OnPurchaseSuccess -= OnPurchaseSuccess;
            MetaIAPManager.Instance.OnPurchaseFailed -= OnPurchaseFailed;
            MetaIAPManager.Instance.OnInitializeFailed -= OnInitFailed;
        }
    }
}