using UnityEngine;
using UnityEditor;

public class StereoImageShaderGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;

        // Find properties
        MaterialProperty stereoMode = FindProperty("_StereoMode", properties);
        MaterialProperty invertStereo = FindProperty("_InvertStereo", properties);

        // Draw default inspector
        base.OnGUI(materialEditor, properties);

        // Add helpful info box based on selected mode
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Stereo Image Settings", EditorStyles.boldLabel);

        int mode = (int)stereoMode.floatValue;
        switch (mode)
        {
            case 0:
                EditorGUILayout.HelpBox("Standard mode: No stereo effect will be applied.", MessageType.Info);
                break;
            case 1:
                EditorGUILayout.HelpBox("Top-Bottom mode: Image should have left eye on top half, right eye on bottom half.", MessageType.Info);
                break;
            case 2:
                EditorGUILayout.HelpBox("Side-by-Side mode: Image should have left eye on left half, right eye on right half.", MessageType.Info);
                break;
        }

        // Show preview warning for non-VR development
        if (mode > 0 && !PlayerSettings.virtualRealitySupported)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("VR is not enabled in Player Settings. Stereo effect will only be visible in VR mode.", MessageType.Warning);
            
            if (GUILayout.Button("Enable VR Support"))
            {
                PlayerSettings.virtualRealitySupported = true;
            }
        }
    }
}
