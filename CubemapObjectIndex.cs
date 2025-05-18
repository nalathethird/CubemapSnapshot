using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using CubemapMaker;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace CubemapMaker
{
    public class CubemapObjectIndex : MonoBehaviour
    {
        [Serializable]
        public class CubemapMetadata
        {
            public string folderPath;        // Path to the cubemap folder
            public string lastCaptureDate;   // When the last capture was taken
            public string objectName;        // Name of the object at time of capture
            public bool isReflectionProbe;   // Whether this was captured from a ReflectionProbe
            public List<string> captureHistory = new List<string>(); // List of capture dates
        }

        [SerializeField, HideInInspector]
        private CubemapMetadata metadata;
        private CubemapSnapshot snapshotComponent;
        
        // Constants for folder structure
        private const string BASE_FOLDER = "CubemapOutput";
        private const string OBJECTS_FOLDER = "Objects";

        private void OnEnable()
        {
            snapshotComponent = GetComponent<CubemapSnapshot>();
            InitializeMetadata();
        }

        private void InitializeMetadata()
        {
            // Create new metadata if needed
            if (metadata == null || string.IsNullOrEmpty(metadata.folderPath) || 
                metadata.folderPath.Equals(BASE_FOLDER) ||
                !metadata.folderPath.Contains(OBJECTS_FOLDER))
            {
                Debug.Log("Creating new metadata with proper folder path");
                
                // Generate a new unique path
                string newPath = GenerateUniqueFolderPath();
                
                // Create new metadata
                metadata = new CubemapMetadata
                {
                    objectName = gameObject.name,
                    isReflectionProbe = GetComponent<ReflectionProbe>() != null,
                    folderPath = newPath
                };
                
                // Verify the folderPath was set correctly
                if (string.IsNullOrEmpty(metadata.folderPath) || 
                    !metadata.folderPath.Contains(OBJECTS_FOLDER))
                {
                    Debug.LogError($"CRITICAL: folderPath was not set correctly: {metadata.folderPath}");
                    // Force a valid path
                    metadata.folderPath = Path.Combine(
                        BASE_FOLDER, 
                        OBJECTS_FOLDER, 
                        $"{gameObject.name}_{DateTime.Now.Ticks}"
                    );
                    Debug.Log($"Forced path to: {metadata.folderPath}");
                }
                
#if UNITY_EDITOR
                // Only mark scene as dirty in editor mode, not play mode
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(this);
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
#endif
            }

            // Create the folder when metadata is first initialized
            string fullPath = GetCubemapFolder();
            if (!Directory.Exists(fullPath))
            {
                try
                {
                    Directory.CreateDirectory(fullPath);
                    SaveMetadataToFolder(fullPath);
                    Debug.Log($"Created cubemap folder at: {fullPath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create cubemap folder: {e.Message}");
                }
            }
        }

        private string GenerateUniqueFolderPath()
        {
            // Sanitize object name for file system
            string safeName = System.Text.RegularExpressions.Regex.Replace(gameObject.name, "[^a-zA-Z0-9]", "_");
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            
            // Create a folder structure:
            // Assets/CubemapOutput/Objects/ObjectName_Timestamp
            string path = Path.Combine(
                BASE_FOLDER, 
                OBJECTS_FOLDER, 
                $"{safeName}_{timestamp}"
            );
            
            Debug.Log($"Generated new folder path: {path}");
            
            return path;
        }

        public string GetCubemapFolder()
        {
            // CRITICAL: Double-check metadata before returning path
            if (metadata == null || string.IsNullOrEmpty(metadata.folderPath) || 
                !metadata.folderPath.Contains(OBJECTS_FOLDER))
            {
                Debug.LogError("CRITICAL: Invalid metadata or folderPath - reinitializing");
                InitializeMetadata();
            }
            
            // Always combine with Assets path to ensure we're inside Unity's project
            string fullPath = Path.Combine(Application.dataPath, metadata.folderPath);
            string absolutePath = Path.GetFullPath(fullPath);
            
            // VERIFY the path is valid
            if (absolutePath == Application.dataPath || !absolutePath.Contains("CubemapOutput"))
            {
                Debug.LogError($"CRITICAL PATH ERROR: GetCubemapFolder returning invalid path: {absolutePath}");
                
                // Force a valid path
                string fixedPath = Path.Combine(
                    Application.dataPath,
                    BASE_FOLDER,
                    OBJECTS_FOLDER,
                    $"EMERGENCY_{DateTime.Now.Ticks}"
                );
                Debug.Log($"Emergency path correction to: {fixedPath}");
                return fixedPath;
            }
            
            // Log the path for debugging
            Debug.Log($"GetCubemapFolder returning: {absolutePath}");
            
            return absolutePath;
        }

        public void PrepareCubemapCapture()
        {
            // Update metadata before capture
            metadata.lastCaptureDate = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            metadata.captureHistory.Add(metadata.lastCaptureDate);
            
            // Log what we're about to do
            Debug.Log($"PrepareCubemapCapture for object: {gameObject.name}");
            
            // Ensure the base folders exist
            EnsureBaseDirectoriesExist();
            
            // Ensure output folder exists
            string outputPath = GetCubemapFolder();
            Debug.Log($"Using output path: {outputPath}");
            
            if (!Directory.Exists(outputPath))
            {
                try
                {
                    Directory.CreateDirectory(outputPath);
                    Debug.Log($"Created cubemap folder: {outputPath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create cubemap folder: {e.Message}");
                }
            }
            
            // Save updated metadata
            SaveMetadataToFolder(outputPath);
        }
        
        // Make sure all base directories exist
        private void EnsureBaseDirectoriesExist()
        {
            string baseDir = Path.Combine(Application.dataPath, BASE_FOLDER);
            Debug.Log($"Ensuring base directory exists: {baseDir}");
            
            if (!Directory.Exists(baseDir))
            {
                Directory.CreateDirectory(baseDir);
                Debug.Log($"Created base folder: {baseDir}");
            }
            
            string objectsDir = Path.Combine(baseDir, OBJECTS_FOLDER);
            Debug.Log($"Ensuring objects directory exists: {objectsDir}");
            
            if (!Directory.Exists(objectsDir))
            {
                Directory.CreateDirectory(objectsDir);
                Debug.Log($"Created objects folder: {objectsDir}");
            }
        }

        public bool HasExistingCaptures()
        {
            string fullPath = GetCubemapFolder();
            if (!Directory.Exists(fullPath)) return false;

            // Check for existing cubemap faces
            string[] faceNames = { "Right", "Left", "Up", "Down", "Front", "Back" };
            string[] extensions = { ".png", ".jpg", ".webp" };

            foreach (string face in faceNames)
            {
                foreach (string ext in extensions)
                {
                    if (File.Exists(Path.Combine(fullPath, face + ext)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SaveMetadataToFolder(string folderPath)
        {
            string metadataPath = Path.Combine(folderPath, "cubemap_metadata.json");
            string json = JsonUtility.ToJson(metadata, true);
            File.WriteAllText(metadataPath, json);
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                try
                {
                    // Only handle folder deletion if we're being removed directly (not through CubemapSnapshot)
                    var snapshot = GetComponent<CubemapSnapshot>();
                    if (snapshot == null)
                    {
                        string fullPath = GetCubemapFolder();
                        if (Directory.Exists(fullPath) && HasExistingCaptures())
                        {
                            if (EditorUtility.DisplayDialog("Delete Cubemap Folder?",
                                "Would you like to delete the associated cubemap folder for this object?",
                                "Yes", "No"))
                            {
                                try
                                {
                                    Directory.Delete(fullPath, true);
                                    Debug.Log($"Deleted cubemap folder: {fullPath}");
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError($"Failed to delete folder: {e.Message}");
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error during cleanup: {e.Message}");
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CubemapObjectIndex))]
public class CubemapObjectIndexEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var indexer = (CubemapObjectIndex)target;
        serializedObject.Update();
        
        EditorGUILayout.HelpBox(
            "This component manages cubemap storage for this object.\n" +
            "It is automatically managed by CubemapSnapshot.",
            MessageType.Info);

        EditorGUI.BeginDisabledGroup(true); // Make everything read-only

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Cubemap Information", EditorStyles.boldLabel);

        var metadataProp = serializedObject.FindProperty("metadata");
        if (metadataProp != null)
        {
            var objectNameProp = metadataProp.FindPropertyRelative("objectName");
            var isReflectionProbeProp = metadataProp.FindPropertyRelative("isReflectionProbe");
            var lastCaptureDateProp = metadataProp.FindPropertyRelative("lastCaptureDate");
            var folderPathProp = metadataProp.FindPropertyRelative("folderPath");
            var captureHistoryProp = metadataProp.FindPropertyRelative("captureHistory");

            if (objectNameProp != null) EditorGUILayout.LabelField("Object Name", objectNameProp.stringValue);
            if (isReflectionProbeProp != null) EditorGUILayout.LabelField("Is Reflection Probe", isReflectionProbeProp.boolValue.ToString());
            if (lastCaptureDateProp != null) EditorGUILayout.LabelField("Last Capture", lastCaptureDateProp.stringValue);
            if (folderPathProp != null) EditorGUILayout.LabelField("Storage Path", folderPathProp.stringValue);

            // Show capture history if any exists
            if (captureHistoryProp != null && captureHistoryProp.arraySize > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Capture History", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (int i = captureHistoryProp.arraySize - 1; i >= 0; i--)
                {
                    var historyEntry = captureHistoryProp.GetArrayElementAtIndex(i);
                    if (historyEntry != null)
                    {
                        EditorGUILayout.LabelField(historyEntry.stringValue);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);
        if (GUILayout.Button("Open Cubemap Folder"))
        {
            string fullPath = indexer.GetCubemapFolder();
            if (Directory.Exists(fullPath))
            {
                EditorUtility.RevealInFinder(fullPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Folder Not Found", 
                    "No cubemap has been generated yet for this object.", 
                    "OK");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif 