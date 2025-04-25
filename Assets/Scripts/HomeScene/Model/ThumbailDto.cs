using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThumbnailsJson
{
    public ThumbnailDTO[] thumbnails;
}

public enum ThumbnailType
{
	LiveShow,
	Aarti,
	Documentary
}

public class ThumbnailDTO
{
    public string show_id;
    public string title;
    public string description = null;
    public string city;
    public int videoDuration;
    public string thumbnailUrl = null;
    //public string thumbnailImgFilePath = null;
    public string thumbnailIconUrl = null;
    //public string thumbnailIconFilePath = null;
    public ShowUrls showUrls;
    public bool isLowQualitySupportedOnDevice;
    public bool isMediumQualitySupportedOnDevice;
    public bool isHighQualitySupportedOnDevice;
    public string highQualityVideoKeyAWSObjectId;
    public string mediumQualityVideoKeyAWSObjectId;
    public string lowQualityVideoKeyAWSObjectId;
    public string AdaptiveStreamVideoKeyAWSObjectId;
    public bool isAarthi;
    public bool isDocumentary;
    public string hindiTitleText;
    public string hindiCityText;
    public string MediumQualityMediaSize;
    public string HighQualityMediaSize;
    public bool isNewVideo;
    public bool isLiveShow;
    public bool isPrevLiveShowThumbnail;
    public string liveFileTimingTextUrl = null;
    public string liveFileTimingText = null;
    public bool isAdaptiveStreamingEnabled;
        

	public ThumbnailType GetThumbnailType()
	{
		if (isLiveShow) return ThumbnailType.LiveShow;
		if (isAarthi) return ThumbnailType.Aarti;
		if (isDocumentary) return ThumbnailType.Documentary;
		return ThumbnailType.Documentary;
	}

    public string toString()
    {
        return $"ThumbnailDTO: {show_id}, {title}, {description}, {city}, {videoDuration}, {thumbnailUrl}, {thumbnailIconUrl}, {showUrls}, {isLowQualitySupportedOnDevice}, {isMediumQualitySupportedOnDevice}, {isHighQualitySupportedOnDevice}, {highQualityVideoKeyAWSObjectId}, {mediumQualityVideoKeyAWSObjectId}, {lowQualityVideoKeyAWSObjectId}, {AdaptiveStreamVideoKeyAWSObjectId}, {isAarthi}, {isDocumentary}, {hindiTitleText}, {hindiCityText}, {MediumQualityMediaSize}, {HighQualityMediaSize}, {isNewVideo}, {isLiveShow}, {isPrevLiveShowThumbnail}, {liveFileTimingTextUrl}, {liveFileTimingText}, {isAdaptiveStreamingEnabled}";
    }

}

public class ShowUrls
{
    public string low;
    public string medium;
    public string high;
    public string adaptive;
}