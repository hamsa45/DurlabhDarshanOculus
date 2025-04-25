public interface IJsonDownloader
{
    void DownloadJson(string url, System.Action<string> onDownloadComplete);
}
