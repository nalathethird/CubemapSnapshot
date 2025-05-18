using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System;
using CubemapMaker;
using CubemapMaker.Editor;

[CustomEditor(typeof(CubemapSnapshot))]
public class CubemapSnapshotEditor : Editor
{
    private bool showAdvancedSettings = false;
    
    // Serialized properties
    private SerializedProperty resolutionProp;
    private SerializedProperty includeSkyboxProp;
    private SerializedProperty cullingMaskProp;
    private SerializedProperty imageFormatProp;
    private SerializedProperty jpgQualityProp;
    private SerializedProperty webpQualityProp;
    private SerializedProperty showDebugLogsProp;
    private SerializedProperty showCapturePreviewProp;

    // Styles
    private GUIStyle _headerStyle;
    private GUIStyle _warningStyle;
    private GUIContent _captureButtonContent;
    private GUIContent _openFolderContent;
    private GUIContent _formatHeaderContent;
    private GUIContent _captureHeaderContent;
    private GUIContent _advancedHeaderContent;

    // Style accessors
    private GUIStyle HeaderStyle
    {
        get
        {
            if (_headerStyle == null) InitializeHeaderStyle();
            return _headerStyle ?? EditorStyles.boldLabel;
        }
    }

    private GUIStyle WarningStyle
    {
        get
        {
            if (_warningStyle == null) InitializeWarningStyle();
            return _warningStyle ?? EditorStyles.helpBox;
        }
    }

    private GUIContent CaptureButtonContent
    {
        get
        {
            if (_captureButtonContent == null) InitializeGUIContent();
            return _captureButtonContent ?? new GUIContent("Capture Cubemap");
        }
    }

    private GUIContent OpenFolderContent
    {
        get
        {
            if (_openFolderContent == null) InitializeGUIContent();
            return _openFolderContent ?? new GUIContent("Open Output");
        }
    }

    private GUIContent FormatHeaderContent
    {
        get
        {
            if (_formatHeaderContent == null) InitializeGUIContent();
            return _formatHeaderContent ?? new GUIContent("Format Settings");
        }
    }

    private GUIContent CaptureHeaderContent
    {
        get
        {
            if (_captureHeaderContent == null) InitializeGUIContent();
            return _captureHeaderContent ?? new GUIContent("Capture Settings");
        }
    }

    private GUIContent AdvancedHeaderContent
    {
        get
        {
            if (_advancedHeaderContent == null) InitializeGUIContent();
            return _advancedHeaderContent ?? new GUIContent("Advanced Settings");
        }
    }

    // Components
    private CubemapObjectIndex objectIndex;
    private CubemapSnapshot snapshot;

    private void OnEnable()
    {
        snapshot = (CubemapSnapshot)target;
        objectIndex = snapshot.GetComponent<CubemapObjectIndex>();

        // Get serialized properties
        resolutionProp = serializedObject.FindProperty("resolution");
        includeSkyboxProp = serializedObject.FindProperty("includeSkybox");
        cullingMaskProp = serializedObject.FindProperty("cullingMask");
        imageFormatProp = serializedObject.FindProperty("imageFormat");
        jpgQualityProp = serializedObject.FindProperty("jpgQuality");
        webpQualityProp = serializedObject.FindProperty("webpQuality");
        showDebugLogsProp = serializedObject.FindProperty("showDebugLogs");
        showCapturePreviewProp = serializedObject.FindProperty("showCapturePreview");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawCubemapHeader();
        DrawFormatSettings();
        DrawCaptureSettings();
        DrawAdvancedSettings();
        DrawActionButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCubemapHeader()
    {
        EditorGUILayout.Space(5);
        
        // Draw the logo if available
        if (ResoniteEditorResources.LogoTexture != null)
        {
            Rect logoRect = EditorGUILayout.GetControlRect(false, 60);
            logoRect.width = 60;
            logoRect.x = EditorGUIUtility.currentViewWidth / 2 - 30;
            ResoniteEditorResources.DrawLogo(logoRect);
            EditorGUILayout.Space(5);
        }
        
        // Title - use local style if Resonite style is not available
        EditorGUILayout.LabelField("CubemapSnapshot", 
            ResoniteEditorResources.HeaderStyle ?? HeaderStyle);
        
        // Description
        EditorGUILayout.HelpBox(
            "A tool for exporting Unity Reflection Probes to Resonite-compatible cubemaps.",
            MessageType.None);
    }

    private void DrawFormatSettings()
    {
        EditorGUILayout.Space(10);
        
        // Draw section header with icon
        GUILayout.BeginHorizontal();
        if (ResoniteEditorResources.FormatIcon != null)
        {
            GUILayout.Label(FormatHeaderContent, GUILayout.Width(25), GUILayout.Height(25));
        }
        EditorGUILayout.LabelField("Format Settings", 
            ResoniteEditorResources.SubHeaderStyle ?? HeaderStyle);
        GUILayout.EndHorizontal();
        
        // Image format dropdown
        EditorGUILayout.PropertyField(imageFormatProp);
        
        // Format-specific settings
        CubemapSnapshot.ImageFormat format = (CubemapSnapshot.ImageFormat)imageFormatProp.enumValueIndex;
        if (format == CubemapSnapshot.ImageFormat.JPG)
        {
            EditorGUILayout.PropertyField(jpgQualityProp, new GUIContent("JPG Quality"));
        }
        else if (format == CubemapSnapshot.ImageFormat.WEBP)
        {
            EditorGUILayout.PropertyField(webpQualityProp, new GUIContent("WebP Quality"));
        }
        
        // About formats button
        if (GUILayout.Button("About Image Formats", ResoniteEditorResources.ButtonStyle ?? GUI.skin.button, GUILayout.Height(24)))
        {
            EditorUtility.DisplayDialog("About Image Formats",
                "PNG: Lossless format with alpha channel support. Largest file size.\n\n" +
                "JPG: Lossy compression without alpha. Good for environments without transparency.\n\n" +
                "WebP: Modern format with good compression and optional alpha. Best for web and VR.",
                "OK");
        }
    }

    private void DrawCaptureSettings()
    {
        EditorGUILayout.Space(10);
        
        // Draw section header with icon
        GUILayout.BeginHorizontal();
        if (ResoniteEditorResources.SettingsIcon != null)
        {
            GUILayout.Label(new GUIContent(ResoniteEditorResources.SettingsIcon), GUILayout.Width(25), GUILayout.Height(25));
        }
        EditorGUILayout.LabelField("Capture Settings", 
            ResoniteEditorResources.SubHeaderStyle ?? EditorStyles.boldLabel);
        GUILayout.EndHorizontal();
        
        // Resolution settings with warning
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(resolutionProp);
        if (EditorGUI.EndChangeCheck() && resolutionProp.intValue > 4096)
        {
            // Check if user has already given high resolution consent
            SerializedProperty consentProp = serializedObject.FindProperty("hasHighResolutionConsent");
            bool hasConsent = consentProp != null && consentProp.boolValue;
            
            // Only show dialog if consent hasn't been given yet
            if (!hasConsent && !EditorUtility.DisplayDialog("High Resolution Warning",
                "Resolutions above 4K (4096) can significantly impact performance and VRAM usage.\n\n" +
                "This setting should only be used for specialized rendering or testing purposes. " +
                "For most cases, 2K (2048) resolution provides excellent quality with reasonable performance.\n\n" +
                "Do you want to proceed with this resolution?",
                "Proceed", "Adjust to 2K"))
            {
                resolutionProp.intValue = 2048;
            }
            else
            {
                // User clicked Proceed or already had consent - set the hidden consent flag
                if (consentProp != null)
                {
                    consentProp.boolValue = true;
                    EditorUtility.SetDirty(snapshot);
                }
            }
        }

        EditorGUILayout.PropertyField(includeSkyboxProp);
        EditorGUILayout.PropertyField(cullingMaskProp);
    }

    private void DrawAdvancedSettings()
    {
        EditorGUILayout.Space(10);
        
        // Use foldout with icon
        GUILayout.BeginHorizontal();
        
        // Create a rect for proper icon alignment
        Rect lineRect = EditorGUILayout.GetControlRect(true, 20);
        Rect iconRect = lineRect;
        iconRect.width = 20;
        iconRect.height = 20;
        
        // Adjust the foldout rect to make space for the icon
        Rect foldoutRect = lineRect;
        foldoutRect.x += 20;
        foldoutRect.width -= 20;
        
        if (ResoniteEditorResources.AdvancedIcon != null)
        {
            GUI.DrawTexture(iconRect, ResoniteEditorResources.AdvancedIcon, ScaleMode.ScaleToFit);
        }
        
        showAdvancedSettings = EditorGUI.Foldout(foldoutRect, showAdvancedSettings, "Advanced Settings", true);
        
        GUILayout.EndHorizontal();
        
        if (showAdvancedSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(showDebugLogsProp);
            EditorGUILayout.PropertyField(showCapturePreviewProp);
            EditorGUI.indentLevel--;
        }
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.Space(15);
        
        // Main action buttons with Resonite styling
        GUILayout.BeginHorizontal();
        
        // Capture button with icon
        if (GUILayout.Button(CaptureButtonContent, 
            ResoniteEditorResources.ButtonStyle ?? GUI.skin.button, GUILayout.Height(30)))
        {
            snapshot.CaptureCubemap();
        }

        // Open folder button with icon
        if (GUILayout.Button(OpenFolderContent, 
            ResoniteEditorResources.ButtonStyle ?? GUI.skin.button, GUILayout.Height(30)))
        {
            if (objectIndex != null)
            {
                string path = objectIndex.GetCubemapFolder();
                if (Directory.Exists(path))
                {
                    // For paths inside the Assets folder, we need to make it a relative path for Unity
                    string assetsPath = Application.dataPath;
                    if (path.StartsWith(assetsPath))
                    {
                        string relativePath = "Assets" + path.Substring(assetsPath.Length);
                        EditorUtility.RevealInFinder(relativePath);
                    }
                    else
                    {
                        EditorUtility.RevealInFinder(path);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Folder Not Found", 
                        "No cubemap has been generated yet for this object.", 
                        "OK");
                }
            }
        }
        GUILayout.EndHorizontal();
    }

    private void InitializeHeaderStyle()
    {
        if (EditorStyles.boldLabel == null) return;
        
        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            margin = new RectOffset(0, 0, 5, 5)
        };
    }

    private void InitializeWarningStyle()
    {
        if (EditorStyles.helpBox == null) return;
        
        _warningStyle = new GUIStyle(EditorStyles.helpBox)
        {
            normal = { textColor = Color.yellow }
        };
    }

    private void InitializeGUIContent()
    {
        _captureButtonContent = new GUIContent("Capture Cubemap", ResoniteEditorResources.CaptureIcon);
        _openFolderContent = new GUIContent("Open Output", ResoniteEditorResources.OpenFolderIcon);
        _formatHeaderContent = new GUIContent(ResoniteEditorResources.FormatIcon);
        _captureHeaderContent = new GUIContent(ResoniteEditorResources.SettingsIcon);
        _advancedHeaderContent = new GUIContent(ResoniteEditorResources.AdvancedIcon);
    }
}
