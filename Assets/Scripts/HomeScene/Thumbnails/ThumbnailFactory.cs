using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// public enum ThumbnailType
// {
// 	LiveShow,
// 	Aarti,
// 	Documentary
// }

//can be simplified using the strategy pattern

// public static class ThumbnailFactory
// {
// 	public static ThumbnailBase CreateVideoThumbnail(ThumbnailType type, Transform parent, GameObject sharedPrefab)
// 	{
// 		GameObject instance = GameObject.Instantiate(sharedPrefab, parent);
// 		instance.TryGetComponent<VideoThumbnailUI>(out VideoThumbnailUI ui);

// 		if(ui == null)
// 		{
// 			Debug.LogWarning("UI is null, adding VideoThumbnailUI to the prefab");
// 			ui = instance.AddComponent<VideoThumbnailUI>();
// 		}

// 		ThumbnailBase logic = type switch
// 		{
// 			ThumbnailType.LiveShow => instance.AddComponent<LiveShowThumbnail>(),
// 			ThumbnailType.Aarti => instance.AddComponent<AartiThumbnail>(),
// 			ThumbnailType.Documentary => instance.AddComponent<DocumentaryThumbnail>(),
// 			_ => instance.AddComponent<DocumentaryThumbnail>()
// 		};

// 		logic.InjectUI(ui);
// 		return logic;
// 	}
// }
