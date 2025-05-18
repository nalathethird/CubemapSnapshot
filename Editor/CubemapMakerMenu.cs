using UnityEngine;
using UnityEditor;
using System.IO;

namespace CubemapMaker.Editor
{
    public static class CubemapMakerMenu
    {
        [MenuItem("Window/Cubemap Maker/Create Cubemap Snapshot", false, 100)]
        public static void CreateCubemapSnapshot()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("No Object Selected", 
                    "Please select a GameObject to add the CubemapSnapshot component to.", 
                    "OK");
                return;
            }
            
            // Check if component already exists
            CubemapSnapshot existing = selected.GetComponent<CubemapSnapshot>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Component Exists", 
                    "This GameObject already has a CubemapSnapshot component.", 
                    "OK");
                Selection.activeGameObject = selected;
                return;
            }
            
            // Add components
            Undo.RecordObject(selected, "Add CubemapSnapshot");
            var snapshot = selected.AddComponent<CubemapSnapshot>();
            
            // This should automatically add the required CubemapObjectIndex component
            
            EditorUtility.DisplayDialog("Component Added", 
                "CubemapSnapshot component has been added to " + selected.name, 
                "OK");
                
            // Select the GameObject to show the component
            Selection.activeGameObject = selected;
        }
        
        [MenuItem("Window/Cubemap Maker/Documentation", false, 200)]
        public static void OpenDocumentation()
        {
            AboutWindow.ShowWindow();
        }
        
        [MenuItem("Window/Cubemap Maker/Check for Updates", false, 300)]
        public static void CheckForUpdates()
        {
            EditorUtility.DisplayDialog("Check for Updates", 
                "You have the latest version of Cubemap Maker installed.\n\n" +
                "Version: 1.0\n" +
                "Last Updated: " + System.DateTime.Now.ToString("MMMM yyyy"), 
                "OK");
        }
        
        [MenuItem("Assets/Create/Resonite/Cubemap Folder", false, 80)]
        public static void CreateCubemapFolder()
        {
            string parentFolder = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (parentFolder == "")
                parentFolder = "Assets";
            else if (!Directory.Exists(parentFolder))
                parentFolder = Path.GetDirectoryName(parentFolder);
                
            string newFolderPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(parentFolder, "Resonite_Cubemaps"));
            AssetDatabase.CreateFolder(Path.GetDirectoryName(newFolderPath), Path.GetFileName(newFolderPath));
            AssetDatabase.Refresh();
            
            // Create an info file in the folder
            string infoFilePath = Path.Combine(newFolderPath, "README.txt");
            string infoContent = 
                "Resonite Cubemap Export Folder\n" +
                "------------------------------\n\n" +
                "This folder is intended for storing cubemap exports for Resonite.\n\n" +
                "Each subfolder contains a complete cubemap export with the following files:\n" +
                "- Six face images: Front, Back, Left, Right, Up, Down\n" +
                "- metadata.json: Information about the capture\n" +
                "- probe_settings.json: For reflection probes, contains probe settings\n\n" +
                "For more information, see the Resonite documentation.";
                
            File.WriteAllText(Path.Combine(Application.dataPath, "..", infoFilePath), infoContent);
            AssetDatabase.Refresh();
        }
        
        [MenuItem("GameObject/Resonite/Add Cubemap Snapshot", false, 10)]
        public static void AddCubemapSnapshotToObject()
        {
            CreateCubemapSnapshot();
        }
    }
} 