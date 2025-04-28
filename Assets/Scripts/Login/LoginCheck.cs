using Newtonsoft.Json;
using UnityEngine;
using co.techxr.unity.model;
using co.techxr.unity.network;
using co.techxr.unity.exceptions;
using System;
using UnityEngine.SceneManagement;
using System.IO;

public class LoginCheck : MonoBehaviour
{
    INetworkService networkService;
    LevelChanger levelChanger;
    // Start is called before the first frame update
    private void Awake()
    {
        Url.setBaseUrl("https://gateway.techxr.co");
        Url.getNetworkService((iNetworkService) => this.networkService = iNetworkService);
        levelChanger = FindObjectOfType<LevelChanger>();
    }

    public void LogInCheck()
    {
        checkLogin();
    }

    private void checkLogin()
    {
         try
        {
            if (PlayerPrefs.GetString("LoginResponse")  == ""|| PlayerPrefs.GetString("LoginResponse") == null)
            {
                Debug.Log("Go to login");
                //Debug.LogWarning(" Current Scene is " + SceneManager.GetActiveScene().name);
                //SceneManager.LoadSceneAsync("SignUp");
                levelChanger.FadeToLevel(1);
            }
            else
            {

                ProjectMetadata.loginResponse = JsonConvert.DeserializeObject<LoginResponse>(PlayerPrefs.GetString("LoginResponse"));
                ProjectMetadata.profile = ProjectMetadata.loginResponse.profile;
                ProjectMetadata.gameDto = JsonConvert.DeserializeObject<GameDto>(PlayerPrefs.GetString("GameDto"));
                LoginResponse loginrrr = JsonConvert.DeserializeObject<LoginResponse>(PlayerPrefs.GetString("LoginResponse"));

                Debug.Log(loginrrr.access_token);
                networkService.setLoginResponse(loginrrr);
                Debug.Log("Go to home");

                levelChanger.FadeToLevel(2);
                //if (!PlayerPrefs.HasKey("ControllerTutorial") || PlayerPrefs.GetInt("ControllerTutorial") == 0)
                //{
                //    SceneManager.LoadSceneAsync("ControllerTutorial");
                //    Debug.Log("Controller Scene");
                //}
                //else
                //{
                //    SceneManager.LoadSceneAsync("HomeScene");
                //    Debug.Log("Home Scene");

                    //}
                    //SceneManager.LoadSceneAsync("Homepage");
                    
            }
        }
        catch(XrException e)
        {
            Debug.Log("Login checking error" + e.Message);

            // SceneManager.LoadScene("SignUp");
            levelChanger.FadeToLevel(1);
        }
        catch (Exception e)
        {
            Debug.Log("Login checking error" + e.Message);

            //SceneManager.LoadScene("SignUp");
            levelChanger.FadeToLevel(1);
        }
    }
}
