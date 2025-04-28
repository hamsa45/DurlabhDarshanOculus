using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AspectRatioFitter), typeof(Graphic))]
public class AutoAdjustImage : MonoBehaviour
{
    public Graphic targetGraphic;  // Reference to the Image or RawImage component you want to adjust
    private AspectRatioFitter aspectRatioFitter;

    public void adjustImage(Graphic targetGraphic)
    {
        if (targetGraphic != null)
        {
            aspectRatioFitter = targetGraphic.gameObject.GetComponent<AspectRatioFitter>();
            if (aspectRatioFitter == null)
            {
                aspectRatioFitter = targetGraphic.gameObject.AddComponent<AspectRatioFitter>();
            }

            // Get the texture based on the component type
            Texture texture = null;
            if (targetGraphic is RawImage rawImage)
            {
                texture = rawImage.texture;
            }
            else if (targetGraphic is Image image)
            {
                texture = image.mainTexture;
            }

            if (texture != null)
            {
                float imageWidth = texture.width;
                float imageHeight = texture.height;

                // Set the aspect ratio of the AspectRatioFitter
                aspectRatioFitter.aspectRatio = imageWidth / imageHeight;

                // Adjust the aspect mode based on the comparison between the image and screen aspect ratios
                SetAspectRatioFitterMode(imageWidth, imageHeight);

                // Optionally, you can set the target's RectTransform size
                AdjustRectTransform(imageWidth, imageHeight);
            }
            else
            {
                Debug.LogError("Graphic component texture is missing.");
            }
        }
    }

  public void adjustImage(string text)
    {
        if (targetGraphic != null)
        {
            aspectRatioFitter = targetGraphic.gameObject.GetComponent<AspectRatioFitter>();
            if (aspectRatioFitter == null)
            {
                aspectRatioFitter = targetGraphic.gameObject.AddComponent<AspectRatioFitter>();
            }

            // Get the texture based on the component type
            Texture texture = null;
            if (targetGraphic is RawImage rawImage)
            {
                texture = rawImage.texture;
            }
            else if (targetGraphic is Image image)
            {
                texture = image.mainTexture;
            }

            if (texture != null)
            {
                float imageWidth = texture.width;
                float imageHeight = texture.height;

                // Set the aspect ratio of the AspectRatioFitter
                aspectRatioFitter.aspectRatio = imageWidth / imageHeight;

                // Adjust the aspect mode based on the comparison between the image and screen aspect ratios
                SetAspectRatioFitterMode(imageWidth, imageHeight);

                // Optionally, you can set the target's RectTransform size
                AdjustRectTransform(imageWidth, imageHeight);

                Debug.Log(text);
            }
            else
            {
                Debug.LogError("Graphic component texture is missing.");
            }
        }
    }

    void SetAspectRatioFitterMode(float imageWidth, float imageHeight)
    {
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

        return;
        // Get the screen's aspect ratio
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenAspectRatio = screenWidth / screenHeight;

        // If the image aspect ratio is greater than the screen's, set the AspectRatioFitter to stretch vertically
        if (screenAspectRatio < (imageWidth / imageHeight))
        {
            // Stretch horizontally, and let the AspectRatioFitter crop vertically
            aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }
        else
        {
            // Stretch vertically, and let the AspectRatioFitter crop horizontally
            aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        }
    }

    void AdjustRectTransform(float imageWidth, float imageHeight)
    {
        // Get the RectTransform of the target Graphic
        RectTransform rectTransform = targetGraphic.GetComponent<RectTransform>();

        // Get the screen dimensions (adjust according to the parent container's size)
        float screenWidth = transform.parent.GetComponent<RectTransform>().sizeDelta.x;
        float screenHeight = transform.parent.GetComponent<RectTransform>().sizeDelta.y;

        // Adjust the size based on the aspect ratio
        float aspectRatio = imageWidth / imageHeight;

        // Determine if the image will stretch horizontally or vertically
        if (screenWidth / screenHeight > aspectRatio)
        {
            // Stretch vertically (screen width is greater than image aspect ratio)
            float scaleFactor = screenHeight / imageHeight;
            rectTransform.sizeDelta = new Vector2(scaleFactor * imageWidth, screenHeight);
        }
        else
        {
            // Stretch horizontally (image aspect ratio is greater than screen width)
            float scaleFactor = screenWidth / imageWidth;
            rectTransform.sizeDelta = new Vector2(screenWidth, scaleFactor * imageHeight);
        }

        // Ensure the image is centered in the RectTransform
        rectTransform.anchoredPosition = Vector2.zero;
    }
}