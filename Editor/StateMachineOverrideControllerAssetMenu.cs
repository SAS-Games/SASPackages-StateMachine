using System.IO;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class StateMachineOverrideControllerAssetMenu
    {
        [MenuItem("Assets/Create/SAS/State Machine Override Controller")]
        public static void CreateStateMachineOverrideController(MenuCommand context)
        {
            string filePath;

            if (Selection.assetGUIDs.Length == 0)
                filePath = "Assets/New State Machine Override Controller.asset";
            else
                filePath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            if (Directory.Exists(filePath))
            {
                filePath += "/New State Machine Override Controller.asset";
            }
            else
            {
                filePath = Path.GetDirectoryName(filePath) + "/New State Machine Override Controller.asset";
            }

            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
            var controller = ScriptableObject.CreateInstance<StateMachineOverrideController>();
            controller.name = Path.GetFileName(filePath);
            AssetDatabase.CreateAsset(controller, filePath);
        }
    }
}
