using UnityEngine;
using UnityEngine.UI;
using System;

public abstract class ThumbnailPanel<T> : MonoBehaviour where T : class
{
    [SerializeField] protected Image thumbnailImage;
    
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
            thumbnailImage.sprite = null;
        }
    }

    protected abstract void UpdateUI();
} 