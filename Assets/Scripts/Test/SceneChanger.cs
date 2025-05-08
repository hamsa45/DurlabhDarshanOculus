using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{

    [SerializeField] TextAsset localJSON;

    private ThumbnailDTO data;
    private void Awake()
    {
        LoadJSON();
    }

    private void LoadJSON() {

        data = JsonUtility.FromJson<ThumbnailDTO>(localJSON.text);

        CurrentVideoDataInPlay.thumbnailDTO = data;
    }

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

    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One) && SceneManager.GetActiveScene().buildIndex == 1)
        {
            ChangeScene();
        }
    }
    public void ChangeScene() {

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }
}
