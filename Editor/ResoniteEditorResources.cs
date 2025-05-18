using UnityEngine;
using UnityEditor;
using System.IO;

namespace CubemapMaker.Editor
{
    public static class ResoniteEditorResources
    {
        private const string ICONS_PATH = "Icons";
        
        // Icons
        public static Texture2D LogoTexture { get; private set; }
        public static Texture2D CaptureIcon { get; private set; }
        public static Texture2D SettingsIcon { get; private set; }
        public static Texture2D OpenFolderIcon { get; private set; }
        public static Texture2D FormatIcon { get; private set; }
        public static Texture2D AdvancedIcon { get; private set; }
        
        // UI Elements
        private static GUIStyle _headerStyle;
        private static GUIStyle _subHeaderStyle;
        private static GUIStyle _buttonStyle;
        
        public static GUIStyle HeaderStyle 
        {
            get 
            {
                if (_headerStyle == null) InitializeHeaderStyle();
                return _headerStyle;
            }
        }
        
        public static GUIStyle SubHeaderStyle 
        {
            get 
            {
                if (_subHeaderStyle == null) InitializeSubHeaderStyle();
                return _subHeaderStyle;
            }
        }
        
        public static GUIStyle ButtonStyle 
        {
            get 
            {
                if (_buttonStyle == null) InitializeButtonStyle();
                return _buttonStyle;
            }
        }
        
        // Colors from Resonite palette
        public static readonly Color ResoniteBlue = new Color(0.1f, 0.4f, 0.9f);
        public static readonly Color ResoniteGreen = new Color(0.2f, 0.8f, 0.4f);
        public static readonly Color ResonitePurple = new Color(0.6f, 0.2f, 0.8f);
        
        static ResoniteEditorResources()
        {
            EditorApplication.delayCall += LoadResources;
        }
        
        private static void LoadResources()
        {
            // Only load once
            EditorApplication.delayCall -= LoadResources;
            
            // Load icons from the Resources folder using Resources.Load
            LogoTexture = Resources.Load<Texture2D>($"{ICONS_PATH}/RSN_Logomark_Color_1080");
            CaptureIcon = Resources.Load<Texture2D>($"{ICONS_PATH}/Color_128_Spotlight");
            SettingsIcon = Resources.Load<Texture2D>($"{ICONS_PATH}/Color_128_Settings");
            OpenFolderIcon = Resources.Load<Texture2D>($"{ICONS_PATH}/Color_128_SaveScreenshot");
            FormatIcon = Resources.Load<Texture2D>($"{ICONS_PATH}/Color_128_Settings_Graphics");
            AdvancedIcon = Resources.Load<Texture2D>($"{ICONS_PATH}/Color_128_Tools");

            // Log loading results for debugging
            Debug.Log($"[ResoniteEditorResources] Loaded resources from {ICONS_PATH}:" +
                $"\nLogo: {(LogoTexture != null ? "Success" : "Failed")}" +
                $"\nCapture: {(CaptureIcon != null ? "Success" : "Failed")}" +
                $"\nSettings: {(SettingsIcon != null ? "Success" : "Failed")}" +
                $"\nFolder: {(OpenFolderIcon != null ? "Success" : "Failed")}" +
                $"\nFormat: {(FormatIcon != null ? "Success" : "Failed")}" +
                $"\nAdvanced: {(AdvancedIcon != null ? "Success" : "Failed")}");
        }
        
        private static void InitializeHeaderStyle()
        {
            if (EditorStyles.boldLabel == null) return;
            
            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(5, 5, 10, 10)
            };
            
            if (LogoTexture != null)
            {
                _headerStyle.normal.background = CreateColorTexture(new Color(0.15f, 0.15f, 0.15f));
                _headerStyle.contentOffset = new Vector2(40, 0);
                _headerStyle.padding = new RectOffset(10, 10, 6, 6);
                _headerStyle.normal.textColor = Color.white;
            }
        }
        
        private static void InitializeSubHeaderStyle()
        {
            if (EditorStyles.boldLabel == null) return;
            
            _subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                margin = new RectOffset(5, 5, 10, 5)
            };
        }

        private static void InitializeButtonStyle()
        {
            if (GUI.skin == null) return;
            
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(10, 10, 5, 5)
            };
            
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.normal.background = CreateColorTexture(ResoniteBlue);
            _buttonStyle.hover.background = CreateColorTexture(new Color(
                ResoniteBlue.r + 0.1f, 
                ResoniteBlue.g + 0.1f, 
                ResoniteBlue.b + 0.1f
            ));
        }
        
        private static Texture2D CreateColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
        
        public static void DrawLogo(Rect position)
        {
            if (LogoTexture != null)
            {
                GUI.DrawTexture(position, LogoTexture, ScaleMode.ScaleToFit);
            }
        }
    }
} 