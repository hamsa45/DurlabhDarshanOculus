using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class StereoUIImage : MonoBehaviour
{
    public enum StereoMode
    {
        None = 0,
        TopBottom = 1,
        SideBySide = 2
    }

    [Header("Stereo Settings")]
    [SerializeField] private StereoMode stereoMode = StereoMode.None;
    [SerializeField] private bool invertEyes = false;
    
    [Tooltip("Optional material reference. If not assigned, will create a new instance.")]
    [SerializeField] private Material stereoMaterial;

    private Image image;
    private Material instanceMaterial;
    private StereoMode lastStereoMode;
    private bool lastInvertValue;

    private void Awake()
    {
        image = GetComponent<Image>();
        
        // Create material instance if needed
        if (stereoMaterial == null)
        {
            // Try to find the shader
            Shader stereoShader = Shader.Find("UI/StereoImage");
            if (stereoShader == null)
            {
                Debug.LogError("StereoUIImage: UI/StereoImage shader not found. Make sure it's included in your project.");
                return;
            }
            
            instanceMaterial = new Material(stereoShader);
            image.material = instanceMaterial;
        }
        else
        {
            // Use the provided material as a base
            instanceMaterial = new Material(stereoMaterial);
            image.material = instanceMaterial;
        }
        
        // Initialize material properties
        UpdateMaterialProperties();
    }

    private void Update()
    {
        // Update material properties if settings changed
        if (lastStereoMode != stereoMode || lastInvertValue != invertEyes)
        {
            UpdateMaterialProperties();
        }
    }

    private void UpdateMaterialProperties()
    {
        if (instanceMaterial != null)
        {
            instanceMaterial.SetInt("_StereoMode", (int)stereoMode);
            instanceMaterial.SetFloat("_InvertStereo", invertEyes ? 1 : 0);
            
            lastStereoMode = stereoMode;
            lastInvertValue = invertEyes;
        }
    }

    public void SetStereoMode(StereoMode mode)
    {
        stereoMode = mode;
        UpdateMaterialProperties();
    }

    public void SetInvertEyes(bool invert)
    {
        invertEyes = invert;
        UpdateMaterialProperties();
    }

    private void OnDestroy()
    {
        // Clean up the instance material
        if (instanceMaterial != null)
        {
            if (Application.isPlaying)
            {
                Destroy(instanceMaterial);
            }
            else
            {
                DestroyImmediate(instanceMaterial);
            }
        }
    }
}
