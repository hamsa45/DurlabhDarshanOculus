using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LocalJsonDownloader : IJsonDownloader
{
	public void DownloadJson(string path, System.Action<string> onLoad)
	{
		if (File.Exists(path))
		{
			onLoad?.Invoke(File.ReadAllText(path));
		}
		Debug.LogError("Failed to load json from local path");
		onLoad?.Invoke(null);
	}
}
