using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using SAS.TagSystem.Editor;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;
using SAS.TagSystem;
using SAS.StateMachineGraph.Utilities;

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
                if (_runtimeStateMachineController == null)
                {
                    var actorSO = new SerializedObject(target);
                    _runtimeStateMachineController = actorSO.FindProperty("m_Controller").objectReferenceValue as RuntimeStateMachineController;
                }
                if (_keys == null)
                {
                    _keys = _runtimeStateMachineController?.keys;
                    _keys = _keys.AddRange(_runtimeStateMachineController?.tags);
                }

                return _keys;
            }
        }

        private RuntimeStateMachineController _runtimeStateMachineController;

        private void OnEnable()
        {
            DrawConfigsList();
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            serializedObject.FindProperty("m_Controller").objectReferenceValue = EditorGUILayout.ObjectField("Controller", serializedObject.FindProperty("m_Controller").objectReferenceValue, typeof(RuntimeStateMachineController), false);

            if (EditorGUI.EndChangeCheck())
            {
                _runtimeStateMachineController = null;
                _configsList = null;
                _keys = null;
                serializedObject.ApplyModifiedProperties();
                DrawConfigsList();
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
