using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using SAS.TagSystem.Editor;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;
using SAS.TagSystem;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(Actor))]
    public class ActorInspector : UnityEditor.Editor
    {
        private ReorderableList _configsList;
        private static string[] Keys => TagList.GetList("Key List");
        private static TagList Instance => TagList.Instance("Key List");

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
                    EditorUtility.DropDown("ConfigDrawer".GetHashCode(), position, Keys, Array.IndexOf(Keys, tag.stringValue), selectedIndex => OnKeySelected(tag, selectedIndex), ShowKeysList);
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

        private void OnKeySelected(SerializedProperty serializedProperty, int index)
        {
            serializedProperty.stringValue = index != -1 ? Keys[index] : string.Empty;
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void ShowKeysList()
        {
            Selection.activeObject = Instance;
        }
    }
}
