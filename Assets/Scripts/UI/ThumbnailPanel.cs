using UnityEngine;
using UnityEngine.UI;
using System;

public abstract class ThumbnailPanel<T> : MonoBehaviour where T : class
{
    [SerializeField] protected RawImage thumbnailImage;
    
    protected T data;

    public virtual void SetData(T data)
    {
        this.data = data;
        UpdateUI();
    }

    public virtual void Clear()
    {
        data = null;
        if (thumbnailImage != null)
        {
            thumbnailImage.texture = null;
        }
    }

    protected abstract void UpdateUI();
} 