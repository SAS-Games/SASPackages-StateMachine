using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(StateMachineModel))]
    public class StateMachineModelInspector : UnityEditor.Editor
    {
        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
          
            var curName = EditorGUI.DelayedTextField(new Rect(50, 30, EditorGUIUtility.currentViewWidth - 60, EditorGUIUtility.singleLineHeight), new GUIContent("State Machine Name"), target.name);
            if (curName != target.name)
            {
                ((StateMachineModel)target).Rename(curName); 
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target));
            }
        }

        public override void OnInspectorGUI()
        {
        }
    }
}
