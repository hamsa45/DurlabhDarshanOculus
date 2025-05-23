using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AccountPage : MonoBehaviour
{

    public static AccountPage ap;

    [SerializeField] TextMeshProUGUI UserName;
    [SerializeField] TextMeshProUGUI onSubslabel;

    [SerializeField] GameObject CheckingText;
    [SerializeField] GameObject SubscribedText;
    [SerializeField] GameObject UnSubscribedText;
    [SerializeField] GameObject UserInfoPanel;


    [SerializeField] GameObject SubscriptionOptionPanel;
    [SerializeField] GameObject TopUpOptionPanel;

    private void Awake()
    {
        if (ap != null)
            Destroy(ap.gameObject);

        ap = this;
    }


    internal void SetSubscriptionStatus(bool isSubscribed) {
        UnSubscribedText.SetActive(!isSubscribed);
        CheckingText.SetActive(false);
        SubscribedText.SetActive(isSubscribed);

        SubscriptionOptionPanel.SetActive(!isSubscribed);
        TopUpOptionPanel.SetActive(isSubscribed);
        onSubslabel.gameObject.SetActive(isSubscribed);
    }

    internal void SetUpSubscriptionText(string SubscriptionDate) {

        onSubslabel.text = $"Subscription End Date: {SubscriptionDate}";
    }

    internal void SetupUser(string username) {

        UserName.text = username;
        UserInfoPanel.SetActive(true);
    }
}
