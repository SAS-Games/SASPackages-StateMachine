using System.IO;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class StateMachineControllerAssetMenu : MonoBehaviour
    {
        [MenuItem("Assets/Create/SAS/State Machine Controller")]
        public static void CreateStateMachineController(MenuCommand context)
        {
            string filePath;

            if (Selection.assetGUIDs.Length == 0)
                filePath = "Assets/New State Machine Controller.asset";
            else
                filePath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            if (Directory.Exists(filePath))
            {
                filePath += "/New State Machine Controller.asset";
            }
            else
            {
                filePath = Path.GetDirectoryName(filePath) + "/New State Machine Controller.asset";
            }

            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);

          //  StateMachineController.CreateAnimatorControllerAtPath(filePath);
            // Copy Asset
            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Resources.Load<StateMachineModel>("State Machine Controller")), filePath);
            var stateMachineModal = AssetDatabase.LoadAssetAtPath<StateMachineModel>(filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(stateMachineModal));
            
            Selection.activeObject = stateMachineModal;
            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(stateMachineModal);
        }
    }
}
