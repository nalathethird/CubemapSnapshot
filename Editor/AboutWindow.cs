using UnityEngine;
using UnityEditor;
using CubemapMaker.Editor;

public class AboutWindow : EditorWindow
{
    private Texture2D logoTexture;
    private Texture2D colorPalette;
    private Vector2 scrollPosition;

    [MenuItem("Window/Cubemap Maker/About")]
    public static void ShowWindow()
    {
        var window = GetWindow<AboutWindow>();
        window.titleContent = new GUIContent("About Cubemap Maker");
        window.minSize = new Vector2(450, 500);
        window.Show();
    }

    private void OnEnable()
    {
        // Load resources
        logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resonite_PressKit_031025/Logos/PNGs/RSN_Logo_Color_1080.png");
        colorPalette = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resonite_PressKit_031025/Color Palette/RSN_ColorPalette.png");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Logo
        if (logoTexture != null)
        {
            var rect = EditorGUILayout.GetControlRect(false, 100);
            rect.x = (position.width - 300) / 2;
            rect.width = 300;
            GUI.DrawTexture(rect, logoTexture, ScaleMode.ScaleToFit);
            EditorGUILayout.Space(10);
        }

        // Title
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Resonite Cubemap Maker", titleStyle);
        EditorGUILayout.Space(5);
        
        // Version
        GUIStyle versionStyle = new GUIStyle(EditorStyles.label);
        versionStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Version 1.0", versionStyle);
        EditorGUILayout.Space(20);
        
        // Description
        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Cubemap Maker is a Unity tool for creating and exporting cubemap textures " +
            "that are compatible with Resonite VR platform. This tool simplifies the " +
            "process of creating high-quality reflection maps for your virtual worlds.",
            MessageType.None);
        EditorGUILayout.Space(10);
        
        // Features
        EditorGUILayout.LabelField("Key Features", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "• High-quality cubemap capture directly from your Unity scene\n" +
            "• Multiple export formats: PNG, JPG, WebP\n" +
            "• Automatic folder organization and metadata tracking\n" +
            "• Reflection probe parameter export\n" +
            "• Preview captures in the editor",
            MessageType.None);
        EditorGUILayout.Space(10);
        
        // Credits
        EditorGUILayout.LabelField("Credits", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool uses assets from the Resonite Press Kit. All Resonite assets " +
            "are property of Resonite and used with permission.\n\n" +
            "Resonite is a powerful social VR platform for creating and exploring virtual worlds. " +
            "Learn more at: https://resonite.com",
            MessageType.None);
        EditorGUILayout.Space(10);
        
        // Color palette
        if (colorPalette != null)
        {
            EditorGUILayout.LabelField("Resonite Color Palette", EditorStyles.boldLabel);
            var rect = EditorGUILayout.GetControlRect(false, 150);
            rect.x = (position.width - 400) / 2;
            rect.width = 400;
            GUI.DrawTexture(rect, colorPalette, ScaleMode.ScaleToFit);
            EditorGUILayout.Space(10);
        }
        
        // Links
        if (GUILayout.Button("Visit Resonite Website", GUILayout.Height(30)))
        {
            Application.OpenURL("https://resonite.com");
        }
        
        if (GUILayout.Button("Resonite Documentation", GUILayout.Height(30)))
        {
            Application.OpenURL("https://wiki.resonite.com");
        }
        
        EditorGUILayout.Space(20);
        EditorGUILayout.EndScrollView();
    }
} 