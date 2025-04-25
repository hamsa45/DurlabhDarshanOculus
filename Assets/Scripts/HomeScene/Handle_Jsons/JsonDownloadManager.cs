using System.Collections.Generic;
using UnityEngine;

public class JsonDownloadManager
{
    private readonly IJsonDownloader _jsonDownloader;

    public JsonDownloadManager(IJsonDownloader jsonDownloader)
    {
        _jsonDownloader = jsonDownloader;
    }

    // Parse Single Object
    public void DownloadAndParseSingle<T>(string url, IJsonParser<T> parser, System.Action<T> onDownloadComplete)
    {
         _jsonDownloader.DownloadJson(url, json =>
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Failed to download JSON.");
                onDownloadComplete?.Invoke(default);
            }

            onDownloadComplete?.Invoke(parser.ParseSingle(json));

            //save the file to local cache
        });
    }

    // Parse List of Objects
    public void DownloadAndParseList<T>(string url, IJsonParser<T> parser, System.Action<List<T>> onDonwloadComplete)
	{
		_jsonDownloader.DownloadJson(url, json =>
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Failed to download JSON.");
                onDonwloadComplete?.Invoke(new List<T>());
            }

            onDonwloadComplete?.Invoke(parser.ParseList(json));
        });
    }
}
