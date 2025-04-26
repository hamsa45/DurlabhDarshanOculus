using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using co.techxr.unity.model;
using co.techxr.unity.network;
using co.techxr.unity.exceptions;
using System;
//using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class SignInManager : MonoBehaviour
{
    TMP_InputField userName, password;
    AuthService authService;

    public CanvasGroup logInWarningPanel;

    public static LoginResponse testresponse;

    public static LoaderXR loaderXR;

    LoginResponse savedLoginResponse = new LoginResponse();

    private void Awake()
    {

    }

    public SignInManager()
    {
        authService = AuthService.getInstance();

    }
    // Start is called before the first frame update
    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        userName = GameObject.FindGameObjectWithTag("username").GetComponent<TMP_InputField>();
        password = GameObject.FindGameObjectWithTag("password").GetComponent<TMP_InputField>();

        loaderXR = new LoaderXR();
        loaderXR.CreateLoader();
    }

    private void SubmitUsernamePassword()
    {
        if(Validate())
        {
            logInWarningPanel.gameObject.SetActive(true);
            loaderXR.StopLoader();
        }
        else
        {
            loaderXR.StopLoader();
        }
    }

    public void LogInWarning()
    {
        loaderXR.StartLoader();
        sendLoginDetails();
    }

    private void sendLoginDetails()
    {
        LoginDto loginDto = new LoginDto();
        loginDto.email = userName.text.Trim();
        loginDto.password = password.text;
        /*        loginDto.email = "abhishekprabhat@debugfactory.com";
                loginDto.password = "4222";*/
        loaderXR.StartLoader();
        authService.login(loginDto, handleLoginResponse, ex=> {
            logInWarningPanel.gameObject.SetActive(false);
            ToastManager.Toast.ErrorToast(ex.message);
            loaderXR.StopLoader();
            Logs.Log(ex.message);
        });
    }

    // public void ChangeScene(string sceneName)
    // {
    //     // SceneManager.LoadScene(index);
    //     SceneLoader.LoadScene(sceneName);
    // }

    private void handleFailure(XrException xrException)
    {
       //ToastManager.Toast.ErrorToast(xrException.message);
        
        //i_Toast_XR.GetComponent<I_Toast_XR1>().ShowToast(false, xrException.message);
    }
    public void handleLoginResponse(LoginResponse loginResponse)
    {
        //loaderXR.StopLoader();

        testresponse = loginResponse;

        ProjectMetadata.profile = loginResponse.profile;
        try
        {
            Logs.Log("Profile: " + loginResponse.profile.firstName);
            
            


            //i_Toast_XR.GetComponent<I_Toast_XR1>().ShowToast(true, string.Format("Received Data for {0}", loginResponse.profile.firstName));

            //if (NetworkService2.loginResponse != null)
            //    Debug.Log("NetworkService2.loginResponse = " + NetworkService2.loginResponse.access_token);
            //else Debug.Log("NetworkService2 AccessToken not populated");

            //  Exchange profile id with blockengine userID
            if (loginResponse.profile == null || loginResponse.profile.id == 0)
            {
                Logs.Log("toast : Fatal Error");
                ToastManager.Toast.ErrorToast("Fatal Error, Please Contact Support");
                //i_Toast_XR.GetComponent<I_Toast_XR1>().ShowToast(false, "Fatal Error, Please Contact Support");
            }
            else
            {
                //FadeOut.Play("");

                savedLoginResponse.access_token = loginResponse.access_token;
                savedLoginResponse.token_type = loginResponse.token_type;
                savedLoginResponse.refresh_token = loginResponse.refresh_token;
                savedLoginResponse.expires_in = loginResponse.expires_in;
                savedLoginResponse.scope = loginResponse.scope;
                savedLoginResponse.profile = loginResponse.profile;
                PlayerPrefs.SetString("LoginResponse", JsonConvert.SerializeObject(savedLoginResponse));
                ProjectMetadata.loginResponse = savedLoginResponse;
                ToastManager.Toast.ErrorToast("Successful Login");

                // SceneManager.LoadScene(AppConstants.Scenes.DeviceSupportCheckAfterLogin);
                SceneLoader.LoadScene(Scenes.Home);
            }
        }
        catch (XrException e)
        {
            // show toast
           Logs.Log("toast : " + e.message);
           ToastManager.Toast.ErrorToast(e.message);
            //Debug.Log(i_Toast_XR + "ksdbjkabdkj");
            //i_Toast_XR.GetComponent<I_Toast_XR1>().ShowToast(false, e.message);
        }
        catch (Exception e)
        {
            // Show 'Unexpected error' message
            Logs.Log("unexpected toast : " + e.Message);
           ToastManager.Toast.ErrorToast(e.Message);
            //i_Toast_XR.GetComponent<I_Toast_XR1>().ShowToast(false, e.Message);
        }
        finally
        {
            logInWarningPanel.gameObject.SetActive(false);
            loaderXR.StopLoader();
        }
    }

    // private void WindowCheck()
    // {
    //         StartCoroutine(StartXR());
    //         string uri = "https://goo.gl/E4cRVt";
    //         Google.XR.Cardboard.Api.SaveDeviceParams(uri);

    //         StopXR();
    // }

	// private void StopXR()
	// {
    //     Debug.Log("Stopping XR...");
    //     UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StopSubsystems();
    //     Debug.Log("XR stopped.");

    //     Debug.Log("Deinitializing XR...");
    //     UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.DeinitializeLoader();
    //     Debug.Log("XR deinitialized.");

    //     Camera.main.ResetAspect();
    //     Camera.main.fieldOfView = 60.0f;

    // }

    // public static IEnumerator StartXR()
    // {
    //     yield return UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.InitializeLoader();
    //     if (UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.activeLoader == null)
    //     {
    //         Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
    //     }
    //     else
    //     {
    //         Debug.Log("Starting XR...");
    //         UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StartSubsystems();
    //         yield return null;
    //     }
    // }
    private bool Validate()
    {
        //Debug.Log(String.Format("#{0}% Length={1}", _descriptionField.text, _descriptionField.text.Length));
        if (userName == null || userName.text.Trim((char)8203).Length == 0)
        {
           ToastManager.Toast.ErrorToast("Enter valid Email/Phone Number");
            //i_Toast_XR.GetComponent<I_Toast_XR1>().ShowToast(false, "Please enter username");

            return false;
        }
        else if (password.text == null || password.text.Trim((char)8203).Length == 0 || password.text.Contains(" ") || password.text.Length < 4)
        {
           ToastManager.Toast.ErrorToast("Password Invalid");

            return false;
        }

        return true;
    }

    public void TogglePasswordVisibility(TMP_InputField inputfield)
    {
        if (inputfield.contentType != TMP_InputField.ContentType.Standard)
        {
            inputfield.contentType = TMP_InputField.ContentType.Standard;
        }
        // hide password
        else
        {
            inputfield.contentType = TMP_InputField.ContentType.Password;
        }
        inputfield.ForceLabelUpdate();
    }

    public void LogIn()
    {
        loaderXR.StartLoader();

        StartCoroutine(CheckInternetConnection(isConnected =>
        {
            if (isConnected)
            {
                SubmitUsernamePassword();
            }
            else
            {
                loaderXR.StopLoader();
                ToastManager.Toast.ErrorToast("Internet Not Available");
            }
        }));
    }
    public static IEnumerator CheckInternetConnection(Action<bool> action)
    {
        UnityWebRequest request = new UnityWebRequest("https://google.com");
        DownloadHandler downloadhandler = request.downloadHandler;
        yield return request.SendWebRequest();
        if (request.error != null)
        {
            action(false);
        }
        else
        {
            action(true);
        }
    }

    #region Drop Down For Country Code

    #endregion
}
