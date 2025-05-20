using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using WebPEncoder = CubemapMaker.WebP.WebPEncoder;
using UnityEditor;
using System.Collections.Generic;

namespace CubemapMaker
{
    [RequireComponent(typeof(CubemapObjectIndex))]
    public class CubemapSnapshot : MonoBehaviour
    {
        private static class Logger
        {
            public static void Log(string message) => Debug.Log($"[CubemapSnapshot] {message}");
            public static void Warning(string message) => Debug.LogWarning($"[CubemapSnapshot] {message}");
            public static void Error(string message) => Debug.LogError($"[CubemapSnapshot] {message}");
        }

        public enum ImageFormat
        {
            PNG,
            JPG,
            WEBP
        }

        [Range(256, 8192)]
        public int resolution = 2048;
        public bool includeSkybox = true;
        public LayerMask cullingMask = -1;
        public ImageFormat imageFormat = ImageFormat.PNG;
        [Range(0, 100)]
        public int jpgQuality = 95;
        [Range(0, 100)]
        public int webpQuality = 95;

        [Header("Debug")]
        public bool showDebugLogs = true;
        public bool showCapturePreview = true;

        // Hidden flag for high resolution consent - only set through dialog
        [SerializeField, HideInInspector]
        private bool hasHighResolutionConsent = false;

        private readonly string[] faceNames = { "Left", "Right", "Top", "Bottom", "Front", "Back" };
        private readonly Vector3[] faceDirections = {
            Vector3.left, Vector3.right, Vector3.up,
            Vector3.down, Vector3.forward, Vector3.back
        };
        private readonly Vector3[] faceUpVectors = {
            Vector3.down, Vector3.down, Vector3.forward,
            Vector3.back, Vector3.down, Vector3.down
        };

        private CubemapObjectIndex objectIndex;

        private void Awake()
        {
            objectIndex = GetComponent<CubemapObjectIndex>();
        }

        // Method to set consent - called from editor dialog
        public void SetHighResolutionConsent(bool consent)
        {
            hasHighResolutionConsent = consent;
        }

        private void OnValidate()
        {
            resolution = Mathf.ClosestPowerOfTwo(resolution);
            if (imageFormat == ImageFormat.WEBP)
            {
                webpQuality = Mathf.Clamp(webpQuality, 0, 100);
            }

            // Ensure we have the required CubemapObjectIndex component
            if (objectIndex == null)
            {
                objectIndex = GetComponent<CubemapObjectIndex>();
                if (objectIndex == null && !Application.isPlaying)
                {
                    objectIndex = gameObject.AddComponent<CubemapObjectIndex>();
                }
            }
        }

        private void OnDestroy()
        {
            // When this component is removed, also remove the CubemapObjectIndex if it exists
            if (!Application.isPlaying && objectIndex != null)
            {
                try
                {
                    // Store the folder path before destroying the component
                    string folderPath = objectIndex.GetCubemapFolder();
                    bool hasCaptures = objectIndex.HasExistingCaptures();

                    // Only ask about folder deletion if there are actual captures
                    if (hasCaptures)
                    {
                        bool deleteFolder = EditorUtility.DisplayDialog("Delete Cubemap Folder?",
                            "Would you like to delete the associated cubemap folder for this object?",
                            "Yes", "No");

                        if (deleteFolder && Directory.Exists(folderPath))
                        {
                            try
                            {
                                Directory.Delete(folderPath, true);
                                Logger.Log($"Deleted cubemap folder: {folderPath}");
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"Failed to delete folder: {e.Message}");
                            }
                        }
                    }

                    // Remove the index component last
                    DestroyImmediate(objectIndex);
                }
                catch (Exception e)
                {
                    Logger.Error($"Error during cleanup: {e.Message}");
                    // Still try to remove the component even if other operations failed
                    DestroyImmediate(objectIndex);
                }
            }
        }

        private string GetFileExtension()
        {
            switch (imageFormat)
            {
                case ImageFormat.JPG:
                    return ".jpg";
                case ImageFormat.WEBP:
                    return ".webp";
                default:
                    return ".png";
            }
        }

        private byte[] EncodeTexture(Texture2D tex)
        {
            try
            {
                switch (imageFormat)
                {
                    case ImageFormat.JPG:
                        return tex.EncodeToJPG(jpgQuality);
                    case ImageFormat.WEBP:
                        try
                        {
                            byte[] webpData = WebPEncoder.EncodeToWebP(tex, webpQuality);
                            if (webpData == null || webpData.Length == 0)
                            {
                                Logger.Error("WebP encoding returned empty data. Falling back to PNG.");
                                return tex.EncodeToPNG();
                            }
                            return webpData;
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"WebP encoding failed: {e.Message}. Falling back to PNG.");
                            return tex.EncodeToPNG();
                        }
                    default:
                        return tex.EncodeToPNG();
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Texture encoding failed: {e.Message}. Falling back to PNG.");
                return tex.EncodeToPNG();
            }
        }

        private class CaptureState
        {
            public string outputPath;
            public Camera[] cameras;
            public RenderTexture[] renderTextures;
            public GameObject[] cameraObjects;
            public int currentStep = 0;
            public bool isCapturing = false;
            public MonoBehaviour context;
        }

        private CaptureState currentCapture;

        public void CaptureCubemap()
        {
            // Validate resolution
            if (resolution > 4096 && !hasHighResolutionConsent)
            {
                Logger.Error("Cannot capture at resolution > 4K without explicit consent via the resolution warning dialog.");
                return;
            }

            // Ensure we have the object index
            if (objectIndex == null)
            {
                objectIndex = GetComponent<CubemapObjectIndex>();
                if (objectIndex == null)
                {
                    Logger.Error("CubemapObjectIndex component is required but not found!");
                    return;
                }
            }

            // Explicitly get the cubemap folder from the index
            string outputPath = objectIndex.GetCubemapFolder();
            Logger.Log($"CaptureCubemap using path: {outputPath}");
            
            // Create output directory if it doesn't exist
            if (!Directory.Exists(outputPath))
            {
                try {
                    Directory.CreateDirectory(outputPath);
                    Logger.Log($"Created output directory: {outputPath}");
                }
                catch (Exception e) {
                    Logger.Error($"Failed to create output directory: {e.Message}");
                    return;
                }
            }

            // Start the capture process
            StartCoroutine(CaptureProcess());
        }

        public void ExportReflectionProbeCubemap(ReflectionProbe probe)
        {
            if (!ValidateSettings() || probe == null)
                return;

            if (currentCapture != null && currentCapture.isCapturing)
            {
                Logger.Warning("Capture already in progress!");
                return;
            }

            try
            {
                // First make sure the object index exists
                if (objectIndex == null)
                {
                    objectIndex = GetComponent<CubemapObjectIndex>();
                    if (objectIndex == null)
                    {
                        Logger.Error("CubemapObjectIndex component is required but not found!");
                        return;
                    }
                }
                
                // Prepare the folder and update metadata
                objectIndex.PrepareCubemapCapture();
                
                // Get the absolute path from the index
                string outputPath = objectIndex.GetCubemapFolder();
                Logger.Log($"Preparing to capture reflection probe to: {outputPath}");
                
                // Verify the folder exists
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                    Logger.Log($"Created output folder: {outputPath}");
                }

                if (objectIndex.HasExistingCaptures() && 
                    !EditorUtility.DisplayDialog("Existing Captures Found",
                        "This reflection probe already has existing captures. Would you like to create a new capture?",
                        "Create New", "Cancel"))
                {
                    return;
                }

                StartCapture(outputPath, probe.transform.position, probe);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to prepare capture: {e.Message}");
            }
        }

        private IEnumerator CaptureProcess()
        {
            if (!ValidateSettings())
                yield break;

            if (currentCapture != null && currentCapture.isCapturing)
            {
                Logger.Warning("Capture already in progress!");
                yield break;
            }

            // Prepare the object index for capture
            try
            {
                // First make sure the object index exists
                if (objectIndex == null)
                {
                    objectIndex = GetComponent<CubemapObjectIndex>();
                    if (objectIndex == null)
                    {
                        Logger.Error("CubemapObjectIndex component is required but not found!");
                        yield break;
                    }
                }
                
                // Prepare the folder and update metadata
                objectIndex.PrepareCubemapCapture();
                
                // Get the absolute path from the index
                string outputPath = objectIndex.GetCubemapFolder();
                Logger.Log($"Preparing to capture cubemap to: {outputPath}");
                
                // Verify the folder exists
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                    Logger.Log($"Created output folder: {outputPath}");
                }

                // Check for existing captures
                if (objectIndex.HasExistingCaptures())
                {
                    bool proceed = EditorUtility.DisplayDialog("Existing Captures Found",
                        "This object already has existing captures. Would you like to create a new capture?",
                        "Create New", "Cancel");
                    
                    if (!proceed)
                        yield break;
                }

                // Start the actual capture with the absolute path
                StartCapture(outputPath, transform.position, GetComponent<ReflectionProbe>());
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to prepare capture: {e.Message}");
                Cleanup();
                yield break;
            }
            
            // Wait for capture to complete
            while (currentCapture != null && currentCapture.isCapturing)
            {
                yield return null;
            }
        }

        // Transform Unity coordinates to Resonite coordinates
        private Vector3 TransformToResoniteCoords(Vector3 unityVector)
        {
            // Unity (Left-handed): Y up, Z forward, X right
            // Resonite (Right-handed): Z up, Y forward, X right
            // To convert from left to right-handed, we need to flip the Z axis
            return new Vector3(
                unityVector.x,   // X stays the same
                unityVector.z,   // Y in Unity becomes Z in Resonite
                -unityVector.y   // Z in Unity becomes -Y in Resonite (flipped for right-handed)
            );
        }

        private Quaternion TransformToResoniteRotation(Quaternion unityRotation)
        {
            // Convert Unity rotation to Resonite rotation
            Vector3 euler = unityRotation.eulerAngles;
            return Quaternion.Euler(
                euler.x,     // X rotation stays the same
                euler.z,     // Y in Unity becomes Z in Resonite
                -euler.y     // Z in Unity becomes -Y in Resonite (flipped for right-handed)
            );
        }

        private void StartCapture(string outputPath, Vector3 position, ReflectionProbe probe)
        {
            // CRITICAL PATH CHECK - Don't use the Assets folder directly
            if (outputPath.Equals(Application.dataPath) || !outputPath.Contains("CubemapOutput"))
            {
                Logger.Error($"CRITICAL PATH ERROR: Attempted to use invalid output path: {outputPath}");
                
                // Get a corrected path from the index
                if (objectIndex != null)
                {
                    outputPath = objectIndex.GetCubemapFolder();
                    Logger.Log($"Corrected path to: {outputPath}");
                }
                else
                {
                    Logger.Error("Cannot proceed with capture - missing index component and invalid path.");
                    return;
                }
            }
            
            // Log the actual output path for debugging
            Logger.Log($"StartCapture using path: {outputPath}");
            
            // Validate resolution
            if (resolution > 4096)
            {
                if (!hasHighResolutionConsent)
                {
                    Logger.Error("Cannot capture at resolution > 4K without explicit consent via the resolution warning dialog.");
                    return;
                }
                else
                {
                    Logger.Log($"Proceeding with high resolution capture ({resolution}x{resolution}) as user has given consent.");
                }
            }

            Logger.Log($"Starting capture at position: {position}");
            EditorUtility.DisplayProgressBar("Preparing Capture", "Setting up cameras...", 0f);

            currentCapture = new CaptureState
            {
                outputPath = outputPath,  // Store the full subfolder path
                cameras = new Camera[6],
                renderTextures = new RenderTexture[6],
                cameraObjects = new GameObject[6],
                context = this,
                isCapturing = true
            };

            // Double check path was set correctly
            Logger.Log($"Set capture output path to: {currentCapture.outputPath}");

            // Set up cameras
            for (int i = 0; i < 6; i++)
            {
                currentCapture.cameraObjects[i] = new GameObject($"CubemapCam_{faceNames[i]}");
                currentCapture.cameraObjects[i].transform.position = position;
                
                // Transform the rotation to Resonite's coordinate system
                Vector3 direction = TransformToResoniteCoords(faceDirections[i]);
                Vector3 up = TransformToResoniteCoords(faceUpVectors[i]);
                Quaternion baseRotation = Quaternion.LookRotation(direction, up);

                // Apply 180-degree rotation ONLY to Top and Bottom faces
                if (i == 2 || i == 3) // Top, Bottom
                {
                    baseRotation *= Quaternion.Euler(0, 180, 0);
                }

                // Apply global -90 degree X rotation
                baseRotation = Quaternion.Euler(-90, 0, 0) * baseRotation;

                currentCapture.cameraObjects[i].transform.rotation = baseRotation;

                var cam = currentCapture.cameraObjects[i].AddComponent<Camera>();
                cam.fieldOfView = 90;
                cam.aspect = 1.0f;
                cam.enabled = false;
                cam.cullingMask = cullingMask;
                cam.clearFlags = includeSkybox ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;

                var rt = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
                rt.antiAliasing = 4;
                cam.targetTexture = rt;

                currentCapture.cameras[i] = cam;
                currentCapture.renderTextures[i] = rt;
            }

            EditorApplication.update += CaptureUpdate;
        }

        private void CaptureUpdate()
        {
            if (currentCapture == null || !currentCapture.isCapturing)
            {
                EditorApplication.update -= CaptureUpdate;
                return;
            }

            try
            {
                switch (currentCapture.currentStep)
                {
                    case 0: // Pause and prepare
                        Time.timeScale = 0;
                        EditorUtility.DisplayProgressBar("Capturing", "Preparing scene...", 0.1f);
                        currentCapture.currentStep++;
                        break;

                    case 1: // Render
                        EditorUtility.DisplayProgressBar("Capturing", "Rendering views...", 0.3f);
                        foreach (var cam in currentCapture.cameras)
                        {
                            cam.Render();
                        }
                        currentCapture.currentStep++;
                        break;

                    case 2: // Process and save
                        ProcessAndSaveCaptures();
                        currentCapture.currentStep++;
                        break;

                    case 3: // Cleanup
                        Cleanup();
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error during capture: {e.Message}");
                Cleanup();
            }
        }

        private void ProcessAndSaveCaptures()
        {
            // CRITICAL PATH CHECK - Don't use the Assets folder directly
            if (currentCapture == null || 
                currentCapture.outputPath.Equals(Application.dataPath) || 
                !currentCapture.outputPath.Contains("CubemapOutput"))
            {
                Logger.Error($"CRITICAL PATH ERROR: Invalid output path: {currentCapture?.outputPath}");
                
                // Try to get a corrected path
                if (objectIndex != null)
                {
                    currentCapture.outputPath = objectIndex.GetCubemapFolder();
                    Logger.Log($"Emergency path correction in ProcessAndSaveCaptures: {currentCapture.outputPath}");
                }
                else
                {
                    Logger.Error("Cannot proceed with processing - path invalid and no index component available.");
                    return;
                }
            }
            
            // Log the path again to ensure it's correct
            Logger.Log($"ProcessAndSaveCaptures using path: {currentCapture.outputPath}");
            
            // Export probe settings if this is a reflection probe
            var probe = GetComponent<ReflectionProbe>();
            if (probe != null)
            {
                ExportProbeSettings(probe, currentCapture.outputPath);
            }

            for (int i = 0; i < 6; i++)
            {
                EditorUtility.DisplayProgressBar("Processing", $"Processing face {i + 1}/6...", 0.5f + (i * 0.08f));
                
                try
                {
                    Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
                    RenderTexture.active = currentCapture.renderTextures[i];
                    tex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                    tex.Apply();

                    if (imageFormat == ImageFormat.WEBP)
                    {
                        tex = FlipTextureVertically(tex);
                    }

                    string filePath = Path.Combine(currentCapture.outputPath, $"{faceNames[i]}{GetFileExtension()}");
                    Logger.Log($"Saving face {faceNames[i]} to: {filePath}");
                    
                    byte[] bytes = EncodeTexture(tex);
                    File.WriteAllBytes(filePath, bytes);

                    if (showCapturePreview)
                    {
                        CreatePreview(tex, i);
                    }

                    Destroy(tex);
                }
                catch (Exception e)
                {
                    Logger.Error($"Error processing face {faceNames[i]}: {e.Message}");
                }
            }
        }

        private void CreatePreview(Texture2D tex, int faceIndex)
        {
            GameObject preview = GameObject.CreatePrimitive(PrimitiveType.Quad);
            preview.transform.position = transform.position + faceDirections[faceIndex] * 2f;
            preview.transform.rotation = Quaternion.LookRotation(-faceDirections[faceIndex], faceUpVectors[faceIndex]);
            preview.transform.localScale = Vector3.one * 0.5f;
            
            Material previewMat = new Material(Shader.Find("Unlit/Texture"));
            previewMat.mainTexture = tex;
            preview.GetComponent<Renderer>().material = previewMat;
            
            Destroy(preview, 5f);
        }

        private void Cleanup()
        {
            if (currentCapture != null)
            {
                RenderTexture.active = null;
                for (int i = 0; i < 6; i++)
                {
                    if (currentCapture.renderTextures[i] != null)
                        Destroy(currentCapture.renderTextures[i]);
                    if (currentCapture.cameraObjects[i] != null)
                        Destroy(currentCapture.cameraObjects[i]);
                }

                Time.timeScale = 1;
                currentCapture.isCapturing = false;
                EditorUtility.ClearProgressBar();
                
                // Make absolutely sure we have the right path
                string completePath = currentCapture.outputPath;
                
                // CRITICAL CHECK - Don't use the Assets folder directly
                if (completePath == null || completePath.Equals(Application.dataPath) || !completePath.Contains("CubemapOutput"))
                {
                    Logger.Error($"CRITICAL PATH ERROR in Cleanup: Invalid path {completePath}");
                    
                    if (objectIndex != null)
                    {
                        completePath = objectIndex.GetCubemapFolder();
                        Logger.Log($"Final emergency path correction in Cleanup: {completePath}");
                    }
                    else
                    {
                        Logger.Error("Unable to determine correct output path for cleanup.");
                    }
                }
                
                if (!string.IsNullOrEmpty(completePath) && completePath != Application.dataPath)
                {
                    Logger.Log($"✅ Capture complete! Files saved to: {completePath}");
                    
                    // Ask user if they want to open the folder, instead of doing it automatically
                    bool openFolder = EditorUtility.DisplayDialog(
                        "Capture Complete",
                        $"Cubemap was successfully captured and saved to:\n{completePath}\n\nWould you like to open this folder now?",
                        "Open Folder",
                        "Not Now");
                    
                    if (openFolder && Directory.Exists(completePath))
                    {
                        string assetsPath = Application.dataPath;
                        if (completePath.StartsWith(assetsPath))
                        {
                            // For paths inside Assets, make it a relative path
                            string relativePath = "Assets" + completePath.Substring(assetsPath.Length);
                            EditorUtility.RevealInFinder(relativePath);
                        }
                        else
                        {
                            EditorUtility.RevealInFinder(completePath);
                        }
                    }
                }
                else
                {
                    Logger.Error("Capture completed but output path was not set correctly!");
                }
                
                currentCapture = null;

                // Refresh the AssetDatabase to show the new files
                AssetDatabase.Refresh();
            }

            EditorApplication.update -= CaptureUpdate;
        }

        private Texture2D FlipTextureVertically(Texture2D original)
        {
            Texture2D flipped = new Texture2D(original.width, original.height, original.format, false);
            
            for (int x = 0; x < original.width; x++)
            {
                for (int y = 0; y < original.height; y++)
                {
                    flipped.SetPixel(x, original.height - 1 - y, original.GetPixel(x, y));
                }
            }
            
            flipped.Apply();
            return flipped;
        }

        private bool ValidateSettings()
        {
            // Update resolution validation to match the consent system
            if (resolution < 256)
            {
                Logger.Error($"Resolution must be at least 256. Current value: {resolution}");
                return false;
            }
            if (resolution > 8192)
            {
                Logger.Error($"Resolution cannot exceed 8192. Current value: {resolution}");
                return false;
            }
            if (resolution > 4096 && !hasHighResolutionConsent)
            {
                Logger.Error("Resolution above 4096 requires explicit user consent.");
                return false;
            }

            if (objectIndex == null)
            {
                Logger.Error("CubemapObjectIndex component is missing!");
                return false;
            }

            return true;
        }

        private void ExportProbeSettings(ReflectionProbe probe, string outputPath)
        {
            if (probe == null) return;

            var settings = new ProbeSettings
            {
                importance = probe.importance,
                intensity = probe.intensity,
                boxProjection = probe.boxProjection,
                blendDistance = probe.blendDistance,
                boxSize = probe.size,
                boxOffset = probe.center,
                resolution = probe.resolution,
                hdr = probe.hdr,
                shadowDistance = probe.shadowDistance,
                backgroundColor = probe.backgroundColor,
                nearClipPlane = probe.nearClipPlane,
                farClipPlane = probe.farClipPlane
            };

            string json = JsonUtility.ToJson(settings, true);
            string settingsPath = Path.Combine(outputPath, "probe_settings.json");
            File.WriteAllText(settingsPath, json);
            Logger.Log($"Exported probe settings to: {settingsPath}");
        }
    }
}
