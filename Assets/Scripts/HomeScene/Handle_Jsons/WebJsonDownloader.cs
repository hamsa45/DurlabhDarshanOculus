using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;

public class WebJsonDownloader : IJsonDownloader
{
    private string downloadedJson;
    public string DownloadJson(string url)
    {
        downloadedJson = string.Empty;
        PersistanceCoroutineRunner.Instance.StartCoroutine(DownloadJsonAsync(url));
        return downloadedJson;  // Return immediately (asynchronous)
    }

    public void DownloadJson(string url, System.Action<string> onDownloadComplete)
	{
        PersistanceCoroutineRunner.Instance.StartCoroutine(DownloadJsonAsync(url, onDownloadComplete));
	}
    private IEnumerator DownloadJsonAsync(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                downloadedJson = request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Failed to download JSON: {request.error}");
            }
        }
    }

    private IEnumerator DownloadJsonAsync(string url, System.Action<string> onDownloadComplete)
	{
        using (UnityWebRequest request = UnityWebRequest.Get(url))
		{
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
			{
                onDownloadComplete?.Invoke(request.downloadHandler.text);
			}
            else
			{
                Debug.LogError($"Failed to download JSON: {request.error}");
                onDownloadComplete?.Invoke(string.Empty);
			}
		}
	}
}

