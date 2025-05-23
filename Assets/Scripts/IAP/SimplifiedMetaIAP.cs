using UnityEngine;
using UnityEngine.UI;

public class SimplifiedMetaIAP : MonoBehaviour
{
    [Header("Product Configuration")]
    [Tooltip("The SKU of the product to purchase")]
    public string productSKU = "com.yourcompany.yourgame.item1";
    [Tooltip("The button to trigger the purchase")]
    public Button purchaseButton;

    internal void InitializeButton()
    {
        if (MetaIAPManager.Instance != null && purchaseButton != null)
        {
            MetaIAPManager.Instance.RegisterButton(productSKU, purchaseButton);

            MetaIAPManager.Instance.OnPurchaseSuccess += OnPurchaseSuccess;
            MetaIAPManager.Instance.OnUserHasActiveSubscriptions += OnUserHasSubscriptions;
        }
        else
        {
            Debug.LogError("MetaIAPManager instance not found or button not assigned!");
        }
    }

    void OnPurchaseSuccess(string sku)
    {
        if (sku == productSKU)
        {
            ApplyPurchaseEffect();
        }
    }

    void OnUserHasSubscriptions(System.Collections.Generic.List<SubscriptionStatus> subscriptions)
    {
        foreach (SubscriptionStatus subscription in subscriptions)
        {
            if (subscription.SKU == productSKU && subscription.IsActive)
            {
                Debug.LogWarning($"OnUserHasSubscriptions {productSKU}");
                ApplySubscriptionEffect(subscription);
                break;
            }
        }
    }

    void ApplyPurchaseEffect()
    {
        Debug.Log($"Purchase effect applied for {productSKU}");
    }

    void ApplySubscriptionEffect(SubscriptionStatus subscription)
    {
        Debug.Log($"Subscription effect applied for {productSKU}");
        Debug.Log($"Subscription expires in {subscription.DaysRemaining} days");
    }

    void OnDestroy()
    {
        if (MetaIAPManager.Instance != null)
        {
            MetaIAPManager.Instance.OnPurchaseSuccess -= OnPurchaseSuccess;
            MetaIAPManager.Instance.OnUserHasActiveSubscriptions -= OnUserHasSubscriptions;
        }
    }
}