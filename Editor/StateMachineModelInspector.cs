using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ReorderableList = UnityEditorInternal.ReorderableList;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;
using SAS.TagSystem.Editor;
using SAS.TagSystem;

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
    }
}
