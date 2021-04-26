using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using SAS.TagSystem.Editor;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(Actor))]
    public class ActorInspector : UnityEditor.Editor
    {
        private ReorderableList _configsList;
        private void OnEnable()
        {
            _configsList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Configs"), true, true, true, true);
            _configsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Configuration List");
            };
            _configsList.drawElementCallback = (Rect position, int index, bool active, bool focused) =>
            {
                var configsRef = serializedObject.FindProperty("m_Configs");
                if (configsRef.arraySize > 0)
                {
                    var data = configsRef.GetArrayElementAtIndex(index).FindPropertyRelative("data");
                    var tag = configsRef.GetArrayElementAtIndex(index).FindPropertyRelative("name");
                
                    position.width /= 2;
                    EditorGUI.ObjectField(position, data, new GUIContent());
                    position.x = position.xMax + 10;
                    position.width -= 10;
                    EditorUtility.DropDown("ConfigDrawer".GetHashCode(), position, TaggerEditor.TagList, Array.IndexOf(TaggerEditor.TagList, tag.stringValue), selectedIndex => OnTagSelected(tag, selectedIndex));
                }
            };
            _configsList.onAddCallback = list =>
            {
                var configsRef = serializedObject.FindProperty("m_Configs");
                configsRef.InsertArrayElementAtIndex(configsRef.arraySize);
                serializedObject.ApplyModifiedProperties();
            };
            _configsList.onRemoveCallback = list =>
            {
                if (list.index >= 0)
                {
                    var configsRef = serializedObject.FindProperty("m_Configs");
                    configsRef.DeleteArrayElementAtIndex(list.index);
                    serializedObject.ApplyModifiedProperties();
                }
            };
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.FindProperty("m_Controller").objectReferenceValue = EditorGUILayout.ObjectField("Controller", serializedObject.FindProperty("m_Controller").objectReferenceValue, typeof(StateMachineModel), false);
            _configsList.DoLayoutList();
        }

        private void OnTagSelected(SerializedProperty serializedProperty, int index)
        {
            serializedProperty.stringValue = index != -1 ? TaggerEditor.TagList[index] : string.Empty;
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
