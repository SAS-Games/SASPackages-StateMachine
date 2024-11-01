using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using SAS.Utilities.TagSystem.Editor;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;
using SAS.StateMachineGraph.Utilities;
using System.Linq;
using SAS.Utilities.BlackboardSystem;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(Actor))]
    public class ActorInspector : UnityEditor.Editor
    {
        private ReorderableList _configsList;
        private string[] _keys;
        private string[] Keys
        {
            get
            {
                _keys = KeyList.Instance().values;
                _keys = _keys.AddRange(KeyList.Instance().values);
                _keys = _keys.Distinct().ToArray();
                return _keys;
            }
        }

        private void OnEnable()
        {
            DrawConfigsList();
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.FindProperty("m_Controller").objectReferenceValue = EditorGUILayout.ObjectField("Controller", serializedObject.FindProperty("m_Controller").objectReferenceValue, typeof(RuntimeStateMachineController), false);
            serializedObject.FindProperty("m_BlackboardData").objectReferenceValue = EditorGUILayout.ObjectField("BlackboardData", serializedObject.FindProperty("m_BlackboardData").objectReferenceValue, typeof(BlackboardData), false);
            SerializedProperty autoInitializeProperty = serializedObject.FindProperty("m_AutoInitialize");
            autoInitializeProperty.boolValue = EditorGUILayout.Toggle("Auto Initialize", autoInitializeProperty.boolValue);

            if (EditorGUI.EndChangeCheck())
            {
                _configsList = null;
                _keys = null;
                serializedObject.ApplyModifiedProperties();
                DrawConfigsList();
                UnityEditor.EditorUtility.SetDirty(target);
                Repaint();
            }
            serializedObject.ApplyModifiedProperties();
            _configsList?.DoLayoutList();
        }

        private void OnKeySelected(SerializedProperty serializedProperty, int index)
        {
            serializedProperty.stringValue = index != -1 ? Keys[index] : string.Empty;
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void DrawConfigsList()
        {
            if (Keys == null)
                return;

            _configsList = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Configs"), true, true, true, true);
            _configsList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Configuration List");
                var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                EditorGUI.LabelField(rect, name);
                var pos = new Rect(rect.width / 2 + 20, rect.y, rect.width / 2, rect.height);
                EditorGUI.LabelField(pos, "Key", style);
            };
            _configsList.drawElementCallback = (Rect position, int index, bool active, bool focused) =>
            {
                var configsRef = serializedObject.FindProperty("m_Configs");
                if (configsRef.arraySize > 0)
                {
                    var data = configsRef.GetArrayElementAtIndex(index).FindPropertyRelative("data");
                    var name = configsRef.GetArrayElementAtIndex(index).FindPropertyRelative("name");

                    position.width /= 2;
                    var selectedObject = EditorGUI.ObjectField(position, "", data.objectReferenceValue, typeof(ScriptableObject), false);
                    if (selectedObject != data.objectReferenceValue)
                    {
                        data.objectReferenceValue = selectedObject;
                        serializedObject.ApplyModifiedProperties();
                    }
                    position.x = position.xMax + 10;
                    position.width -= 10;
                    EditorUtility.DropDown("ConfigDrawer".GetHashCode(), position, Keys, Array.IndexOf(Keys, name.stringValue), selectedIndex => OnKeySelected(name, selectedIndex));
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
    }
}
