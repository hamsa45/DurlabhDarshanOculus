using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A UI component that represents a single purchasable product with loading state.
/// Attach this to a button GameObject that has a purchase button.
/// </summary>
public class SingleProductPurchaseButton : MonoBehaviour
{
    [Header("Product Configuration")]
    [Tooltip("The SKU of the product to purchase")]
    public string productSKU = "com.yourcompany.yourgame.item1";
    
    [Header("UI References")]
    [Tooltip("Optional text component to show product state (owned/purchasable)")]
    public TextMeshProUGUI buttonText;
    [Tooltip("Loading animation object to show during purchase")]
    public GameObject loadingSpinner;
    [Tooltip("The purchase button component")]
    public Button purchaseButton;
    
    private bool isPurchaseInProgress = false;
    
    void Start()
    {
        // Make sure we have a button component
        if (purchaseButton == null)
            purchaseButton = GetComponent<Button>();
            
        if (purchaseButton == null)
        {
            Debug.LogError("No Button component found on this GameObject");
            return;
        }
        
        // Add onClick listener
        purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
        
        // Initially hide loading spinner
        if (loadingSpinner != null)
            loadingSpinner.SetActive(false);
            
        // Subscribe to IAP events
        if (MetaIAPManager.Instance != null)
        {
            MetaIAPManager.Instance.OnPurchaseSuccess += OnPurchaseSuccess;
            MetaIAPManager.Instance.OnPurchaseFailed += OnPurchaseFailed;
            
            // Check if product is already owned
            CheckProductOwnership();
        }
        else
        {
            Debug.LogError("MetaIAPManager instance not found!");
            DisableButton("IAP Not Available");
        }
    }
    
    void CheckProductOwnership()
    {
        if (MetaIAPManager.Instance.DoesUserOwnProduct(productSKU))
        {
            DisableButton("Owned");
        }
        else
        {
            EnableButton();
        }
    }
    
    void OnPurchaseButtonClicked()
    {
        if (isPurchaseInProgress || MetaIAPManager.Instance == null)
            return;
            
        StartPurchase();
    }
    
    private void StartPurchase()
    {
        isPurchaseInProgress = true;
        
        // Show loading animation
        if (loadingSpinner != null)
            loadingSpinner.SetActive(true);
            
        // Update button text if available
        if (buttonText != null)
            buttonText.text = "Processing...";
            
        // Disable button interaction during purchase
        if (purchaseButton != null)
            purchaseButton.interactable = false;
            
        // Start the purchase process
        MetaIAPManager.Instance.PurchaseProduct(productSKU);
    }
    
    private void OnPurchaseSuccess(string sku)
    {
        // Only process if this is our product
        if (sku != productSKU)
            return;
            
        isPurchaseInProgress = false;
        
        // Hide loading animation
        if (loadingSpinner != null)
            loadingSpinner.SetActive(false);
            
        DisableButton("Owned");
        
        // You can add any additional visual feedback here
        // e.g., a checkmark animation, sound effect, etc.
    }
    
    private void OnPurchaseFailed(string errorMsg)
    {
        isPurchaseInProgress = false;
        
        // Hide loading animation
        if (loadingSpinner != null)
            loadingSpinner.SetActive(false);
            
        EnableButton();
        
        // You could show error feedback here
        Debug.LogError($"Purchase failed: {errorMsg}");
    }
    
    private void EnableButton()
    {
        if (purchaseButton != null)
            purchaseButton.interactable = true;
            
        // Reset button text if available
        if (buttonText != null)
            buttonText.text = "Purchase";
    }
    
    private void DisableButton(string reason)
    {
        if (purchaseButton != null)
            purchaseButton.interactable = false;
            
        // Update text with reason if available
        if (buttonText != null)
            buttonText.text = reason;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (MetaIAPManager.Instance != null)
        {
            MetaIAPManager.Instance.OnPurchaseSuccess -= OnPurchaseSuccess;
            MetaIAPManager.Instance.OnPurchaseFailed -= OnPurchaseFailed;
        }
    }
}