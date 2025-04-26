using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoGridManager : GridManager<ThumbnailDTO>
{
    // This class inherits all functionality from GridManager<VideoData>
    // You can add any video-specific functionality here if needed
    
    // public List<VideoData> videos;
    // void Start()
    // {
    //     LoadVideos(videos);
    // }
    public void LoadVideos(List<ThumbnailDTO> videos)
    {
        LoadItems(videos);
    }
} 