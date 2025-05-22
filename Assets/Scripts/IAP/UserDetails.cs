using Oculus.Platform;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class UserDetails : MonoBehaviour
{
    public TMPro.TextMeshProUGUI debugText;
    public TMPro.TextMeshProUGUI debug2Text;
    private void Start()
    {
        GetUserID();
        GetMetaQuestUserId();
    }


    void GetUserID()
    {
        Users.GetLoggedInUser().OnComplete(message =>
        {
            if (!message.IsError)
            {
                var user = message.Data;
                string userId = user.ID.ToString();
                string username = user.OculusID;
                debugText.text = $"User ID {userId} | User Name {username}";
                // Store this ID for your database tracking
                //StoreUserInDatabase(userId, username);
            }
        });
    }

    void GetMetaQuestUserId()
    {
        Oculus.Platform.Users.GetLoggedInUser().OnComplete(message =>
        {
            if (!message.IsError)
            {
                debug2Text.text = "Debug 2" + message.Data.ID.ToString();
            }
            else
            {
                Debug.LogError("Failed to get Meta Quest User ID");
                //userId = "guest_" + UnityEngine.Random.Range(1000, 9999); // Fallback
            }
        });
    }



    void GetDeviceInfo()
    {
        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.HeadMounted))
            {
                string deviceId = device.serialNumber;
                // Use this along with user authentication
            }
        }
    }
}
