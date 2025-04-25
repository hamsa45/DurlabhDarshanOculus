using System.Collections;
using System.Collections.Generic;
using System.IO;
using co.techxr.unity.network;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEngine.XR.Management;
using Newtonsoft.Json;
using UnityEngine.U2D;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Events;

public class HomeManager : MonoBehaviour
{
    public GameObject name;
    public GameObject email;
    public GameObject phoneNumber;
    AuthService authService;
    public GameObject ThumbnailPrefab;
    public GameObject ARCoreThumbnailPrefab;
    public GameObject ThumbnailParentContent;
    public GameObject ARCubeThumbnailParentContent;
    public GameObject LaunchPage;
    public GameObject NetworkErrorPage;
    public Button retryButton;
    //public RateOurApp rateOurApp;
    [SerializeField] private SpriteAtlas iconAtlas;
    [SerializeField] private SpriteAtlas thumbnailAtlas;
    //[SerializeField] private GameObject ScreenOrientationPopUp;
    //[SerializeField] private GameObject VrBoxInstructionsPopUp;
    //[SerializeField] private GameObject ActivationPanel;
    //[SerializeField] private GameObject QRPopUps;
    [SerializeField] private GameObject ActivationPopUps;
    [SerializeField] private GameObject AccountsPage;
    [SerializeField] private GameObject SubscribedInfo;
    [SerializeField] private TextMeshProUGUI remainingDays;
    [SerializeField] private TextMeshProUGUI purchasedStartDate;
    [SerializeField] private TextMeshProUGUI purchaseEndDate;
    [SerializeField] private GameObject UnsubscribedInfo;
    //[SerializeField] private GameObject QrScan;
    //[SerializeField] private GameObject InputField;

    public static bool isSubscriptionChecked = false;
    [SerializeField] private GameObject subscriptionCheckLoader;
    [SerializeField] private GameObject AR_SceneLoader_Parent;
    [SerializeField] private Image AR_SceneLoader_FillImg;
    //[SerializeField] private GameObject MediaPlayer;
//<<<<<<< HEAD
    [SerializeField] private GameObject DeviceConfigStatus;
    [SerializeField] private GameObject DeviceConfigStatusAccountsPage;
    [SerializeField] private GameObject nonSupportedInfoText;
//=======
    //[SerializeField] private TMP_InputField UserQRTextInput;
    [SerializeField] private Button purchaseNow_btn;
    //[SerializeField] private QRCodeReaderDemo qrReaderDemo;
    //[SerializeField] private CategoryManagement categoryManagement;
    [SerializeField] private TMP_Text appVersion_txt;
//>>>>>>> Feature-LogoutAllOtherDevices

    private bool isProfilePagePopulatedFirstTime = false;
    //private GameObject parentTransform;

    private bool isHighQualityVideoSupportedOnDevice = false;
    private bool isMediumQualityVideoSupportedOnDevice = false;
    private bool isAdaptiveStreamingVideoSupportedOnDevice = false;
    [HideInInspector] public bool isBeginVideoPlayerPressed = false;
    public bool hasToLoadVRScene = true;

    public static event Action OnThumbnailsPopulated;
    public static event Action OnInternetConnectionRestored;
    public static bool isInternetConnected = false;
    #region durlabhdarshan help desk phone number
    private readonly string scanCodeHelpContactNumber = "9109198788"; // critical string , make changes with proper approval !!!!!!!
    #endregion


    //private bool isDeviceOrientationSetToLandscape = false;

    private ThumbnailDTO freeDemoThumbnailDTO;
    private float _networkCheckTimeOut = 5.0f;

    private GameObject retryText;
    private GameObject checkingConnectionText;
    private bool thumbnailsPopulationStarted = false;

    private List<ThumbnailDTO> thumbnailDTOs = new List<ThumbnailDTO>();
    
    public List<ThumbnailDTO> ThumbnailDTOs => thumbnailDTOs;
    private void Awake()
    {
        //Screen.brightness = 0.4f;
        //StopXR();
        //Screen.orientation = ScreenOrientation.Portrait;
        authService = AuthService.getInstance();
        if (ProjectMetadata.profile != null)
        {
            name.GetComponent<TextMeshProUGUI>().text = ProjectMetadata.profile.firstName;
            email.GetComponent<TextMeshProUGUI>().text = ProjectMetadata.profile.email;
            phoneNumber.GetComponent<TextMeshProUGUI>().text = ProjectMetadata.profile.phone.phoneNumber;
        }

        checkInternetConnectionOnAwake();
        if(isInternetConnected || NetworkMonitor.Instance.IsConnected)
        {
            Debug.LogWarning("internet is connected on awake");
            if(!thumbnailsPopulationStarted)
            {
                thumbnailsPopulationStarted = true;
                initializeHomepage();
            }
        }

        retryButton.onClick.AddListener(()=> { RetryInternetCheck(); });
        retryText = retryButton.transform.GetChild(0).gameObject;
        checkingConnectionText = retryButton.transform.GetChild(1).gameObject;
    }

    private void checkInternetConnectionOnAwake()
    {
        retryButton.interactable = false;
        StartCoroutine(InternetConnection(isConnected =>
        {
            if (isConnected)
            {
                NetworkErrorPage.SetActive(false);
                retryButton.interactable = true;
                isInternetConnected = true;
                //uploadSessionDetails();
		        //uploadUnsavedWatchTime();
            }
            else
            {
                NetworkErrorPage.SetActive(true);
                retryButton.interactable = true;
                isInternetConnected = false;
                if(thumbnailsPopulationStarted)
                {
                    //clear all live show checks (clear all live show url checker items)
                    //clear all thumbnails (gameobjects)
                    //clear all categories (thumbnail references)
                    clearHomepage();
                    thumbnailsPopulationStarted = false;
                }
                OnInternetConnectionRestored += initializeHomepage;
            }
        }));
    }

    private void clearHomepage()
    {
        //clear all thumbnails
        Debug.LogWarning("clearing all thumbnails");
        //categoryManagement.clearAllCategories();
        //GameManager.Instance.liveshowManager.ClearAllLiveShowChecks();
        deleteAllThumbnailGOs();
        thumbnailDTOs.Clear();
        Debug.LogWarning("All thumbnails cleaned.");
    }

    private void deleteAllThumbnailGOs()
    {
        foreach (Transform child in ThumbnailParentContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void initializeHomepage()
	{
        // to be done later
		//validateUser();

		#region Populate Thumbnails data
		//updateFreeDemoThumbnailDTO();
		//populateWeeklyLiveThumbnails();
		//populateARThumbnails();
		populateMenuFromInAppData();
		#endregion

		

        //give user offline saved section
	}

	public void RetryInternetCheck()
    {
        retryButton.interactable = false;
        isInternetConnectionChecking(true);

        StartCoroutine(checkInternetConnection(isConnected =>
        {
            if (isConnected)
            {
                NetworkErrorPage.SetActive(false);
                OnInternetConnectionRestored?.Invoke();
                isInternetConnected = true;
            }
            else
            {
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                NetworkErrorPage.SetActive(true);
                isInternetConnected = false;
            }

            isInternetConnectionChecking(false);
        }));
    }

    private void isInternetConnectionChecking(bool isChecking)
    {
        retryText.SetActive(!isChecking);
        checkingConnectionText.SetActive(isChecking);
        retryButton.interactable = !isChecking;
    }

    private IEnumerator InternetConnection(Action<bool> action)
    {
        UnityWebRequest request = new UnityWebRequest("https://google.com");
        //DownloadHandler downloadhandler = request.downloadHandler;
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

    // private void updateApplicationVersion()
	// {
    //     appVersion_txt.text = Application.version;
    // }

    // private void OnEnable()
	// {
    //     SubscribeToQREvents();
	// }

	// private void SubscribeToQREvents()
	// {
    //     UserContentManager.instance.userLoggedInElseWhere += askToLoginHere;
    // }

	// private void askToLoginHere()
	// {
    //     Debug.Log(" asking to log in here or not and showing logout of all other devices ++++");
    //     ActivationPopUps.SetActive(true);
    //     ActivationPopUps.transform.GetChild(0).gameObject.SetActive(true);
    //     foreach (Transform child in ActivationPopUps.transform.GetChild(0))
    //     {
    //         if (child.gameObject.name == "LogOutFromOtherDevices")
    //         {
    //             child.gameObject.SetActive(true);
    //         }
    //         else
    //         {
    //             child.gameObject.SetActive(false);
    //         }
    //     }
    // }

	// private void OnDisable()
    // {
    //     unSubscribeToQREvents();
    // }

    // private void unSubscribeToQREvents()
    // {
    //     UserContentManager.instance.userLoggedInElseWhere -= askToLoginHere;
    // }

    private void Start()
    {
        //Screen.orientation = ScreenOrientation.Portrait;

		//updateApplicationVersion();
		//unloadAssetBundle();
		//showDeviceSupportedVideoFormats();

        //purchaseNow_btn.onClick.AddListener(delegate { purchaseNow(); });
    }

	// private void unloadAssetBundle()
	// {
    //     //create an assetbundle manager and assign this work to him
	// 	GameManager.Instance.unloadAssetBundle();
	// }

	// private void uploadSessionDetails()
	// {
	// 	UserContentManager.instance.uploadSessionDetailsUploadAtStart();
	// }

	// private void validateUser()
	// {
    //     //update to fail fast
	// 	if (!UserContentManager.instance.isSubscribed || !UserContentManager.instance.isSubscribedBefore)
	// 	{
	// 		if (!UserContentManager.instance.getIsUserSubCheckedOnce())
	// 		{
	// 			isSubscriptionChecked = false;
	// 			subscriptionCheckLoader.SetActive(true);

	// 			UserContentManager.instance.checkIfPurchasedUser(c =>
	// 			{
	// 				isSubscriptionChecked = true;
	// 				subscriptionCheckLoader.SetActive(false);
	// 				OnSubscriptionCheck();

	// 				askForAppRating();
	// 			});
	// 		}
	// 	}
	// }

	private void OnSubscriptionCheck() 
    {

        ///to do handle user subscription status---------
        


        // Transform bg = ActivationPopUps.transform.GetChild(0);

        // if (!bg.gameObject.activeSelf) return;

        // foreach (Transform child in bg.transform)
        // {
        //     if (child.gameObject.name == "CheckingUserData")
        //     {
        //         if (!child.gameObject.activeSelf) return;
        //         else child.gameObject.SetActive(false);
        //     }
        // }


        // if (UserContentManager.instance.isSubscribedBefore)
        // {
        //     foreach (Transform child in bg.transform)
        //     {
        //         if (child.gameObject.name == "LogOutFromOtherDevices")
        //         {
        //             child.gameObject.SetActive(true);
        //         }
        //         else
        //         {
        //             child.gameObject.SetActive(false);
        //         }
        //     }
        // }
        // else
        // {
        //     foreach (Transform child in bg.transform)
        //     {
        //         if (child.gameObject.name == "ActivateToWatch")
        //         {
        //             child.gameObject.SetActive(true);
        //         }
        //         else
        //         {
        //             child.gameObject.SetActive(false);
        //         }
        //     }
        // }
    }

	// private void askForAppRating()
	// {
    //     if (GameManager.Instance.shouldAskUserForAppRating()) rateOurApp.gameObject.SetActive(true);
    // }

	// public void DownloadARCubePdf()
    // {
    //     URLHelper.OpenUrl("https://s3-ap-south-1.amazonaws.com/co.techxr.system.backend.upload.dev/M6aV7tXL0d5Fborp_2024-07-19T130332481317.ARCube.pdf");
    // }

    // internal void EnableDisableARLoaderParent(bool isActive)
	// {
    //     AR_SceneLoader_Parent.SetActive(isActive);
	// }

    // internal void AR_loaderFillImage(float fillAmount)
    // {
    //     AR_SceneLoader_FillImg.fillAmount = fillAmount;
    // }

    // private void populateARThumbnails()
	// {
    //     List<ARCoreThumbnailDTO> thumbnails = ARCardsThumbnailsData.GetARCoreThumbnailDTOs();

    //     foreach (var item in thumbnails)
    //     {
    //         Console.WriteLine($"Title: {item.videoTitle}, Orientation: {item.ScreenOrientation}");
    //         GameObject currentARCardthumbnail = Instantiate(ARCoreThumbnailPrefab, ARCubeThumbnailParentContent.transform);
    //         ARCoreThumbnailCardData currentCardData = currentARCardthumbnail.GetComponent<ARCoreThumbnailCardData>();

    //         currentCardData.updateDataAndInitialize(item);
    //     }
    // }

    // private void uploadUnsavedWatchTime()
	// {
	// 	try
	// 	{
	// 		if (PlayerPrefs.HasKey("unsavedWatchedTime"))
	// 		{
	// 			string value = PlayerPrefs.GetString("unsavedWatchedTime");
	// 			string videoTitle = value.Split('.')[0];
	// 			int time = int.Parse(value.Split('.')[1]);
	// 			Debug.Log($"sending video title and time :{value}++++");
	// 			UserContentManager.instance.uploadWatchedTime(time, videoTitle);
	// 			PlayerPrefs.DeleteKey("unsavedWatchedTime");
	// 		}
	// 	}
	// 	catch (Exception ex)
	// 	{
    //         Debug.Log(ex.Message + "*+++ is the error message while updating unsaved watch time.");
	// 	}
	// }

	// private void showDeviceSupportedVideoFormats()
    // {
    //     //mediaFormat qualitySupported = GameManager.instance.DeviceSupportMediaQuality;
    //     isHighQualityVideoSupportedOnDevice = (PlayerPrefs.GetString("deviceSupportHighQualityVideo", "Na") == "true") ? true : false;
    //     isMediumQualityVideoSupportedOnDevice = (PlayerPrefs.GetString("deviceSupportMediumQualityVideo", "Na") == "true") ? true : false;
    //     isAdaptiveStreamingVideoSupportedOnDevice = (PlayerPrefs.GetString("deviceSupportAdaptiveStreamVideo", "Na") == "true") ? true : false;
    //     //isLowQualityVideoSupportedOnDevice = (PlayerPrefs.GetString("deviceSupportLowQualityVideo", "Na") == "true") ? true : false;

    //     string deviceConfigStatus = GameManager.Instance.deviceSupportText_English[2];//"Your device does not support ";
    //     GameObject Bg_DeviceConfigs = new();

    //     //ActivationPopUps.SetActive(true);
    //     foreach (Transform child in ActivationPopUps.transform)
    //     {
    //         if (child.gameObject.name == "Bg-DeviceConfigs")
    //         {
    //             Bg_DeviceConfigs = child.gameObject;
    //         }
    //     }

    //     GameObject deviceStatusPopUp = Bg_DeviceConfigs.transform.GetChild(0).gameObject;

    //     bool isDeviceConfigsPopUpAlreadyShown = (PlayerPrefs.GetString("DeviceConfigsPopUpAlreadyShown", "Na") == "true");
    //     if (!isDeviceConfigsPopUpAlreadyShown)
    //     {
    //         Bg_DeviceConfigs.SetActive(true);
    //         Debug.Log("Device pop up is not already shown.");
    //     }
    //     else
    //     {
    //         Debug.Log("Device pop up is not already shown.");
    //     }

    //     GameObject accountsPageDeviceConfigGO = DeviceConfigStatusAccountsPage.transform.GetChild(1).gameObject;
    //     TMP_Text deviceConfigAccountsPageText = accountsPageDeviceConfigGO.GetComponent<TMP_Text>();

    //     if (isHighQualityVideoSupportedOnDevice)
    //     {
    //         Debug.Log("High quality supported on device++++");
    //         //GameObject watchButton = DeviceConfigStatus.transform.GetChild(0).gameObject;

    //         //text.SetActive(false);
    //         //text = DeviceConfigStatusAccountsPage.transform.GetChild(1).gameObject;
    //         accountsPageDeviceConfigGO.SetActive(false);
    //         deviceStatusPopUp.SetActive(false);
    //         Bg_DeviceConfigs.SetActive(false);
    //         nonSupportedInfoText.SetActive(false);
    //         return;
    //     }
    //     else 
    //     {
    //         deviceStatusPopUp.SetActive(true);
    //         PlayerPrefs.SetString("DeviceConfigsPopUpAlreadyShown", "true");
    //         GameObject compatableGO = deviceStatusPopUp.transform.GetChild(0).gameObject;
    //         GameObject NotcompatableGO = deviceStatusPopUp.transform.GetChild(1).gameObject;
    //         //text.SetActive(true);
    //         if (isMediumQualityVideoSupportedOnDevice)
	// 		{
	// 			//Debug.Log(deviceConfigStatus + "--is the device config. status");
    //             compatableGO.SetActive(true);
    //             NotcompatableGO.SetActive(false);
	// 			deviceConfigStatus = GameManager.Instance.deviceSupportText_English[0];//"Your device supports HD quality";
    //             nonSupportedInfoText.SetActive(true);
    //             nonSupportedInfoText.GetComponent<TMP_Text>().text = deviceConfigStatus;
    //             nonSupportedInfoText.GetComponent<ChangeTextLanguage>().englishText = deviceConfigStatus;
    //             nonSupportedInfoText.GetComponent<ChangeTextLanguage>().hindiText = GameManager.Instance.deviceSupportText_Hindi[0];

    //             //accounts page device support text
    //             deviceConfigAccountsPageText.text = deviceConfigStatus;
    //             deviceConfigAccountsPageText.gameObject.GetComponent<ChangeTextLanguage>().englishText = deviceConfigStatus;
    //             deviceConfigAccountsPageText.gameObject.GetComponent<ChangeTextLanguage>().hindiText = GameManager.Instance.deviceSupportText_Hindi[0];
    //         }
    //         else if (isAdaptiveStreamingVideoSupportedOnDevice)
	// 		{
    //             Debug.Log(deviceConfigStatus + "--is the device config. status");
    //             compatableGO.SetActive(true);
    //             NotcompatableGO.SetActive(false);
    //             deviceConfigStatus = GameManager.Instance.deviceSupportText_English[1];//"Your device supports Adaptive streaming";
    //             nonSupportedInfoText.SetActive(true);
    //             nonSupportedInfoText.GetComponent<TMP_Text>().text = deviceConfigStatus;
    //             nonSupportedInfoText.GetComponent<ChangeTextLanguage>().englishText = deviceConfigStatus;
    //             nonSupportedInfoText.GetComponent<ChangeTextLanguage>().hindiText = GameManager.Instance.deviceSupportText_Hindi[1];
    //             deviceConfigAccountsPageText.text = deviceConfigStatus;
    //             deviceConfigAccountsPageText.gameObject.GetComponent<ChangeTextLanguage>().englishText = deviceConfigStatus;
    //             deviceConfigAccountsPageText.gameObject.GetComponent<ChangeTextLanguage>().hindiText = GameManager.Instance.deviceSupportText_Hindi[1];
    //         }

    //         if (deviceConfigStatus == GameManager.Instance.deviceSupportText_English[2])
	// 		{
    //             compatableGO.SetActive(false);
    //             NotcompatableGO.SetActive(true);
    //             nonSupportedInfoText.SetActive(true);
    //             nonSupportedInfoText.GetComponent<TMP_Text>().text = deviceConfigStatus;
    //             nonSupportedInfoText.GetComponent<ChangeTextLanguage>().englishText = deviceConfigStatus;
    //             nonSupportedInfoText.GetComponent<ChangeTextLanguage>().hindiText = GameManager.Instance.deviceSupportText_Hindi[2];
    //             if (GameManager.Instance.DeviceSupportMediaQuality != mediaFormat.NOTSUPPORTED)
    //             {
    //                 GameManager.Instance.DeviceSupportMediaQuality = mediaFormat.NOTSUPPORTED;
    //             }
    //             deviceConfigAccountsPageText.text = deviceConfigStatus;
    //             deviceConfigAccountsPageText.gameObject.GetComponent<ChangeTextLanguage>().englishText = deviceConfigStatus;
    //             deviceConfigAccountsPageText.gameObject.GetComponent<ChangeTextLanguage>().hindiText = GameManager.Instance.deviceSupportText_Hindi[2];
    //         }
    //     }
    // }

    // public void purchaseNow()
	// {
    //     //Application.OpenURL(AppData.PurchaseUrl);
    //     URLHelper.OpenUrl(AppData.PurchaseUrl);
    // }

    // public void logOutOfAllOtherDevices()
	// {
    //     UserContentManager.instance.logOutOfAllOtherDevices();
	// }

    // private void updateFreeDemoThumbnailDTO()
    // {
    //     Dictionary<string, string> thumbnailData =
    //         FreeDemoThumbnailInAppData.freeDemoThumbnails["0"];

    //     freeDemoThumbnailDTO = convertThumbnailDatatoThumbnailDTO(
    //         thumbnailData, "0");

    // }

	// private void populateWeeklyLiveThumbnails()
	// {
	// 	foreach (var day in GameManager.Instance.liveshowManager.DateToAllLiveShowLinksMap)
	// 	{
	// 		PopulateThumbnailsForTime(day.Value.AMLinks, string.Concat(day.Key,"AM"));
	// 		PopulateThumbnailsForTime(day.Value.PMLinks, string.Concat(day.Key,"PM"));
	// 	}
	// }

	// private void PopulateThumbnailsForTime(LiveShowMediaLinks _liveShowMediaLinks, string key)
	// {
	// 	if (GameManager.Instance.liveshowManager.ShouldPopulateThumbnailForTime(key))
	// 	{
	// 		ThumbnailDTO thumbnailDTO = convertThumbnailDatatoThumbnailDTO(ThumbnailDataInApp.LiveDarshanWeeklythumbnailData["1"], "5");
    //         LiveShowMediaLinks liveShowMediaLinks = _liveShowMediaLinks;
	// 		fillAndInstantiateThumbnailData(thumbnailDTO, liveShowMediaLinks, key);
	// 	}
	// }

    private void populateMenuFromInAppData()
	{
		try
		{
            Debug.Log("Downloading json from cloud.");
			//using url
			var manager = new JsonDownloadManager(new WebJsonDownloader());
			var videoParser = new JsonParser<Dictionary<string, Dictionary<string, string>>>();
			string videoUrl = AppConstants.VideoThumbnailJsonUrl;

			manager.DownloadAndParseSingle(videoUrl, videoParser, json =>
			{
                if (json == null)
                {
                    Debug.LogError("Downloaded JSON is null.");
                    //loadFromResources();
                    return;
                }

                List<ThumbnailDTO> thumbnailDTOsList = new List<ThumbnailDTO>();
			    Dictionary<string, Dictionary<string, string>> jsonData = json;
                
			    foreach (var thumbnail in jsonData)
			    {
			    	//if (AppConstants.disactiveVideoShowIds.Contains(thumbnail.Key) || thumbnail.Key == "5") continue;
			    	ThumbnailDTO thumbnailDTO = convertThumbnailDatatoThumbnailDTO(thumbnail.Value, thumbnail.Key);
                    Logs.Log(thumbnailDTO.toString());
			    	// updateDeviceSupportedQuality(thumbnailDTO);
			    	// fillAndInstantiateThumbnailData(thumbnailDTO);
                    thumbnailDTOsList.Add(thumbnailDTO);
			    }

                this.thumbnailDTOs = thumbnailDTOsList;
			
                OnThumbnailsPopulated?.Invoke();
            });
		}
		catch
		{
            Logs.LogWarning("Error loading the video data from json");
            loadFromResources();
        }
    }

    private void loadFromResources()
	{
        TextAsset textAsset = Resources.Load<TextAsset>("VideoThumbnails");
        string json = textAsset.text;

        Dictionary<string, Dictionary<string, string>> thumbnailDTOs = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
        List<ThumbnailDTO> thumbnailDTOsList = new List<ThumbnailDTO>();
        foreach (var thumbnail in thumbnailDTOs)
        {
            ThumbnailDTO thumbnailDTO = convertThumbnailDatatoThumbnailDTO(thumbnail.Value, thumbnail.Key);
            //updateDeviceSupportedQuality(thumbnailDTO);
            //fillAndInstantiateThumbnailData(thumbnailDTO);
            thumbnailDTOsList.Add(thumbnailDTO);
        }
        this.thumbnailDTOs = thumbnailDTOsList;
        OnThumbnailsPopulated?.Invoke();
    }

	// private void fillAndInstantiateThumbnailData(ThumbnailDTO dto)
	// {
	// 	ThumbnailType type = dto.GetThumbnailType();

    //     //Instantiate thumbnail
    //     ThumbnailBase thumbnail = ThumbnailFactory.CreateVideoThumbnail(type, ThumbnailParentContent.transform, ThumbnailPrefab);//AartiThumbnailPrefab, DocumentaryThumbnailPrefab);

	// 	if (thumbnail == null)
	// 	{
	// 		Debug.LogError("Thumbnail could not be created from factory.");
	// 		return;
	// 	}

	// 	// Inject shared dependencies
	// 	//thumbnail.launchPageGO = LaunchPage.GetComponent<LaunchPageData>();
	// 	thumbnail.networkErrorPage = NetworkErrorPage;
	// 	thumbnail.activationPopUps = ActivationPopUps;
    //     thumbnail.categoryManagement = categoryManagement;
	// 	thumbnail.manager = this.gameObject;

    //     // if (thumbnail is LiveShowThumbnail liveShowThumbnail && liveLinks != null && liveShowTimeText != null)
    //     // {
    //     //     liveShowThumbnail.UpdateMediaLinks(liveLinks);
    //     //     liveShowThumbnail.UpdateLiveShowTimeText(liveShowTimeText);
    //     // }

    //     // Initialize thumbnail
	// 	thumbnail.Initialize(dto, true);

	// }

// 	private void fillAndInstantiateThumbnailData(ThumbnailDTO thumbnailDTO, LiveShowMediaLinks liveShowMediaLinks = null, string liveShowtimeText = null)
//     {
//         GameObject current;
//         current = Instantiate(ThumbnailPrefab, ThumbnailParentContent.transform);
//         ThumbnailData td = current.GetComponent<ThumbnailData>();

//         td.LaunchPageGO = LaunchPage;
//         td.NetworkErrorPage = NetworkErrorPage;

//         td.ActivationPopUps = ActivationPopUps;
//         td.Manager = this.gameObject;
//         td.categoryManagement = this.categoryManagement;

//         if (liveShowMediaLinks is not null)
//         {
// #if UNITY_ANDROID
//             thumbnailDTO.liveFileTimingTextUrl = liveShowMediaLinks.UploadTimeText;
//             thumbnailDTO.quality.mediumQualityVideoURL = liveShowMediaLinks.MediaQuality_2K;
//             thumbnailDTO.quality.highQualityVideoURL = liveShowMediaLinks.MediaQuality_4K;
// #elif UNITY_IOS
//             thumbnailDTO.liveFileTimingTextUrl = liveShowMediaLinks.UploadTimeText;
//             thumbnailDTO.quality.NonEn_mediumQualityVideoURL = liveShowMediaLinks.MediaQuality_2K;
//             thumbnailDTO.quality.NonEn_highQualityVideoURL = liveShowMediaLinks.MediaQuality_4K;
// #endif
// 		}
// 		if (liveShowtimeText is not null) td.liveshowTimeText = liveShowtimeText;

//         td.CreateThumbnail(thumbnailDTO, true);
//         td.checkAndRemoveAtInitialization();

//         categoryManagement.AddToCategory(thumbnailDTO.GetThumbnailType(), current.gameObject, thumbnailDTO.show_id);

//         if (thumbnailDTO.isAdaptiveStreamingEnabled) current.transform.SetAsFirstSibling();
//         if (thumbnailDTO.isNewVideo) current.transform.SetAsFirstSibling();
//     }

    // public void updateDeviceSupportedQuality(ThumbnailDTO thumbnailDTO)
    // {
    //     string defaultValue = "NA";
    //     string baseKey = "videoquality." + thumbnailDTO.show_id + ".";
    //     bool isHighQualitySupportedOnDevice = (PlayerPrefs.GetString(baseKey + "High", defaultValue) == "True") ? true : false;
    //     bool isMediumQualitySupportedOnDevice = (PlayerPrefs.GetString(baseKey + "Medium", defaultValue) == "True") ? true : false;
    //     bool isLowQualitySupportedOnDevice = (PlayerPrefs.GetString(baseKey + "Low", defaultValue) == "True") ? true : false;

    //     thumbnailDTO.isHighQualitySupportedOnDevice = isHighQualitySupportedOnDevice;
    //     thumbnailDTO.isMediumQualitySupportedOnDevice = isMediumQualitySupportedOnDevice;
    //     thumbnailDTO.isLowQualitySupportedOnDevice = isLowQualitySupportedOnDevice;

    // }

    private ThumbnailDTO convertThumbnailDatatoThumbnailDTO(Dictionary<string, string> thumbnailData, string show_id)
    {
        ThumbnailDTO thumbnailDTO = new ThumbnailDTO();

        thumbnailDTO.show_id = show_id;
        thumbnailDTO.title = thumbnailData.GetValueOrDefault("videoTitle", "xyz");
        thumbnailDTO.city = thumbnailData.GetValueOrDefault("city", "xyz");
        thumbnailDTO.description = thumbnailData.GetValueOrDefault("description", "xyz");
        thumbnailDTO.videoDuration = int.Parse(thumbnailData.GetValueOrDefault("videoDuration", "0"));
        thumbnailDTO.highQualityVideoKeyAWSObjectId = thumbnailData.GetValueOrDefault("highQualityVideoKeyAWSObjectId", "xyz");
        thumbnailDTO.mediumQualityVideoKeyAWSObjectId = thumbnailData.GetValueOrDefault("mediumQualityVideoKeyAWSObjectId", "xyz");
        thumbnailDTO.lowQualityVideoKeyAWSObjectId = thumbnailData.GetValueOrDefault("lowQualityVideoKeyAWSObjectId", "xyz");
        thumbnailDTO.AdaptiveStreamVideoKeyAWSObjectId = thumbnailData.GetValueOrDefault("AdaptiveStreamVideoKeyAWSObjectId", "xyz");
        thumbnailDTO.thumbnailIconUrl = thumbnailData.GetValueOrDefault("thumbnailIconUrl", "xyz");
        thumbnailDTO.thumbnailUrl = thumbnailData.GetValueOrDefault("thumbnailUrl", "xyz");

        ShowUrls showUrls = new ShowUrls();
        showUrls.low = thumbnailData.GetValueOrDefault("lowQualityVideoUrl", "xyz");
        showUrls.medium = thumbnailData.GetValueOrDefault("mediumQualityVideoUrl", "xyz");
        showUrls.high = thumbnailData.GetValueOrDefault("highQualityVideoUrl", "xyz");
        showUrls.adaptive = thumbnailData.GetValueOrDefault("AdaptiveStreamingVideoURL", "xyz");

        thumbnailDTO.showUrls = showUrls;
        thumbnailDTO.isAarthi = (thumbnailData.GetValueOrDefault("isAarthi", "xyz") == "true");
        thumbnailDTO.isDocumentary = (thumbnailData.GetValueOrDefault("isDocumentary", "xyz") == "true");
        thumbnailDTO.hindiTitleText = thumbnailData.GetValueOrDefault("hindiTitleText", "xyz");
        thumbnailDTO.hindiCityText = thumbnailData.GetValueOrDefault("hindiCityText", "xyz");
        thumbnailDTO.isNewVideo = thumbnailData.GetValueOrDefault("isNew", "xyz") == "true";

        thumbnailDTO.MediumQualityMediaSize = thumbnailData.GetValueOrDefault("MediumQualityMediaSize", "xyz");
        thumbnailDTO.HighQualityMediaSize = thumbnailData.GetValueOrDefault("HighQualityMediaSize", "xyz");
        thumbnailDTO.isLiveShow = thumbnailData.GetValueOrDefault("isLiveShow", "false") == "true";
        thumbnailDTO.liveFileTimingTextUrl = thumbnailData.GetValueOrDefault("TextFileUrl", "xyz");
        thumbnailDTO.liveFileTimingText = thumbnailData.GetValueOrDefault("liveFileTimingText", "xyz");
        thumbnailDTO.isPrevLiveShowThumbnail = thumbnailData.GetValueOrDefault("isPrevLiveShowThumbnail", "false") == "true";
        thumbnailDTO.isAdaptiveStreamingEnabled = thumbnailData.GetValueOrDefault("isAdaptiveStreamingEnabled", "false") == "true";

        return thumbnailDTO;
    }

    // public void onClickWatchButtonDemo()
    // {
    //     Debug.Log("launch page listener is called");

    //     ThumbnailDTO thumbnailData = new ThumbnailDTO();

    //     var freeDemoThumbnailData = FreeDemoThumbnailInAppData.freeDemoThumbnails["0"];

    //     thumbnailData = convertThumbnailDatatoThumbnailDTO(freeDemoThumbnailData, "0");

    //     string demoVideoSprite = ThumbnailDataInApp.thumbnails["1"]["videoTitle"];

    //     Debug.Log("$$$$$$$$$$ " + demoVideoSprite);

    //     LaunchPage.GetComponent<LaunchPageData>().populateLaunchPage(thumbnailData);
    // }

    // public void OnMapBtnClicked() 
    // {
    //     SceneManager.LoadScene(6);
    // }

    public IEnumerator checkInternetConnection(Action<bool> syncResult)
    {
        const string echoServer = "https://google.com";

        bool result = false;

        while (!result)
        {
            using (var request = UnityWebRequest.Head(echoServer))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();
                result = request.result == UnityWebRequest.Result.Success;
                Debug.Log("internet is connected : " + result);
                //yield return new WaitForSeconds(2);
            }
        }

        // if (result)
        // {
        //     UserContentManager.instance.InitializeFirebase(callback =>
        //     {
        //         //loader stop
        //         if (callback)
        //         {
        //             result = true;
        //         }
        //         else
        //         {
        //             result = false;
        //         }
        //     });
        // }

        yield return new WaitForSeconds(1);

        syncResult(result);

        //if (result)
        //{
        //    yield break;
        //}
    }

    // private void StopXR()
    // {
    //     if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
    //     {
    //         XRGeneralSettings.Instance.Manager.StopSubsystems();
    //         Camera.main.ResetAspect();
    //         XRGeneralSettings.Instance.Manager.DeinitializeLoader();
    //     }
    // }

    public void Logout()
    {
        authService.logout();
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(AppConstants.Scenes.Login);
    }

    // in app subscription
    public void onClickActivateProduct()
    {
        SceneManager.LoadScene(AppConstants.Scenes.QRActivation);
    }

    public void onClickAccountsPage()
    {
        // if (isProfilePagePopulatedFirstTime)
        //     return;

        // isProfilePagePopulatedFirstTime = true;

        // bool isUserSubscribed = UserContentManager.instance.isSubscribed;
        // bool isUserSubscribedBefore = UserContentManager.instance.isSubscribedBefore;

        // AccountsPage.SetActive(true);
        // Debug.Log($"clicked on accounts page and user sub status is : {isUserSubscribed}");
        // if(isUserSubscribed || isUserSubscribedBefore)
        // {
        //     SubscribedInfo.SetActive(true);
        //     UnsubscribedInfo.SetActive(false);

        //     // todo:
        //     // get purchased date from firebase
        //     // add subscription period (1 year currently) in above date
        //     // check the days remaining from current date time c# library
        //     // change the texts of appropiate tmp pro gameobjects
        //     if (remainingDays.text.Contains("-"))
		// 	{
        //         UserContentManager.instance.populateSubscriptionInfo(remainingDays, purchasedStartDate, purchaseEndDate);
        //     }
        // }
        // else
        // {
        //     SubscribedInfo.SetActive(false);
        //     UnsubscribedInfo.SetActive(true);
        // }
    }

    /// <summary>
    /// help button on activation page
    /// </summary>
    public void onClickHelp()
    {
        URLHelper.OpenUrl("tel:+91" + scanCodeHelpContactNumber);
    }
}
