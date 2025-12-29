using System.IO;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public static class StateModelAssetMenu
    {
        [MenuItem("Assets/Create/SAS/FSM/State Model")]
        public static void CreateStateModel(MenuCommand context)
        {
            string basePath;

            // Determine base path
            if (Selection.assetGUIDs.Length == 0)
            {
                basePath = "Assets";
            }
            else
            {
                basePath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
                if (!Directory.Exists(basePath))
                    basePath = Path.GetDirectoryName(basePath);
            }

            // Check if selected object is a StateModel
            StateModel selectedStateModel = Selection.activeObject as StateModel;

            if (selectedStateModel != null)
            {
                DuplicateStateModel(selectedStateModel, basePath);
            }
            else
            {
                CreateNewStateModel(basePath);
            }
        }

        // -----------------------
        // Create new StateModel
        // -----------------------
        private static void CreateNewStateModel(string folderPath)
        {
            string path = Path.Combine(folderPath, "New State.asset");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            var asset = ScriptableObject.CreateInstance<StateModel>();
            asset.name = "New State";

            AssetDatabase.CreateAsset(asset, path);
            FinalizeCreation(asset);
        }

        // -----------------------
        // Duplicate selected StateModel
        // -----------------------
        private static void DuplicateStateModel(StateModel source, string folderPath)
        {
            string sourcePath = AssetDatabase.GetAssetPath(source);
            string fileName = Path.GetFileNameWithoutExtension(sourcePath);
            string path = Path.Combine(folderPath, $"{fileName} Copy.asset");
            path = AssetDatabase.GenerateUniqueAssetPath(path);

            AssetDatabase.CopyAsset(sourcePath, path);

            var duplicated = AssetDatabase.LoadAssetAtPath<StateModel>(path);
            FinalizeCreation(duplicated);
        }

        private static void FinalizeCreation(StateModel asset)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
