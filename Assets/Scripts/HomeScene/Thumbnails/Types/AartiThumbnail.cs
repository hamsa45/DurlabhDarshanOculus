using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// public class AartiThumbnail : ThumbnailBase
// {
// 	protected override void CustomInitialize(ThumbnailDTO dto)
// 	{
// 		GetComponent<Button>().onClick.AddListener(OnThumbnailClick);
		
// 		ThumbnailType type = dto.GetThumbnailType();
// 		ui.setThumbnailTypeIcon(type);
// 		ui.setLiveShowTimingIcon(type == ThumbnailType.LiveShow);
// 		ui.setDurationIcon(type != ThumbnailType.LiveShow);
// 		ui.setPrevLiveShowLoader(type == ThumbnailType.LiveShow);
// 	}

// 	protected override void OnThumbnailClick()
// 	{
// 		launchPageGO.GetComponent<LaunchPageData>().populateLaunchPage(thumbnailData);
// 		CheckAndAdjustNewVideoTag();
// 	}

// 	protected override void ValidateClickAndPlay()
// 	{
// 		base.ValidateClickAndPlay();
// 	}
// }

