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
    [CustomEditor(typeof(StateModel))]
    public class StateModelInspector : UnityEditor.Editor
    {
        private ReorderableList _stateActions;
        private ReorderableList _transitionStates;
        private Type[] _allActionTypes;
        private StateTransitionInspector _stateTransitionInspector;

        private static string[] Tags => TagList.GetList();
        private static string[] Keys => TagList.GetList("Key List");

        private void OnEnable()
        {
            _stateTransitionInspector = new StateTransitionInspector();
            SetupTransitions();
            _allActionTypes = AppDomain.CurrentDomain.GetAllDerivedTypes<IStateAction>().ToArray();
            _stateActions = new ReorderableList(serializedObject, serializedObject.FindProperty("m_StateActions"), true, true, true, true);
            HandleReorderableActionsList(_stateActions, "State Actions");
        }

        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();

            var curName = EditorGUI.DelayedTextField(new Rect(50, 30, EditorGUIUtility.currentViewWidth - 60, EditorGUIUtility.singleLineHeight), new GUIContent("State Name"), target.name);
            if (curName != target.name)
            {
                var runtimeStateMachineController = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target)) as RuntimeStateMachineController;
                ((StateModel)target).Rename(runtimeStateMachineController, curName);
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_stateTransitionInspector == null || !_stateTransitionInspector.OnInspectorGUI())
            {
                if (!target.name.Equals(Util.AnyStateModelName))
                    _stateActions.DoLayoutList();
                _transitionStates.DoLayoutList();
            }

            serializedObject.ApplyModifiedProperties();
            Repaint();
        }

        private void HandleReorderableActionsList(ReorderableList reorderableList, string name)
        {
            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                EditorGUI.LabelField(rect, name);
                var pos = new Rect(rect.width - Mathf.Min(140, rect.width / 2) -20, rect.y, Mathf.Min(90, rect.width / 3), rect.height);
                EditorGUI.LabelField(pos, "Tag", style);
                pos = new Rect(rect.width - Mathf.Min(70, rect.width / 3 - 20), rect.y, Mathf.Min(90, rect.width / 3), rect.height);
                EditorGUI.LabelField(pos, "Key", style);
            };
            reorderableList.onAddCallback = list =>
            {
                reorderableList.serializedProperty.InsertArrayElementAtIndex(reorderableList.serializedProperty.arraySize);
                var fullName = reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.serializedProperty.arraySize - 1).FindPropertyRelative("fullName");
                if (reorderableList.serializedProperty.arraySize == 1)
                    fullName.stringValue = _allActionTypes[0].AssemblyQualifiedName;
            };
            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var actionFullName = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("fullName");
                var tag = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("tag");
                var key = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("key");
                rect.y += 2;
                var curActionIndex = Array.FindIndex(_allActionTypes, type => type.AssemblyQualifiedName == actionFullName.stringValue);
                var pos = new Rect(rect.x, rect.y - 2, Mathf.Max(rect.width - 180, 70), rect.height - 2);
                int id = GUIUtility.GetControlID("actionFullName".GetHashCode(), FocusType.Keyboard, pos);
                EditorUtility.DropDown(id, pos, _allActionTypes.Select(type => SerializedType.Sanitize(type.ToString())).ToArray(), curActionIndex, selectedIndex => SetSelectedAction(actionFullName, selectedIndex));

                pos = new Rect(rect.width - Mathf.Min(140, rect.width / 2), rect.y - 2, Mathf.Min(90, rect.width / 3), rect.height - 2);
                id = GUIUtility.GetControlID("Tag".GetHashCode(), FocusType.Keyboard, pos);
                EditorUtility.DropDown(id, pos, Tags, Array.IndexOf(Tags, tag.stringValue), selectedIndex => SetTagSerializedProperty(tag, selectedIndex));

                pos = new Rect(rect.width - Mathf.Min(50, rect.width / 3 - 40), rect.y - 2, Mathf.Min(90, rect.width / 3), rect.height);
                id = GUIUtility.GetControlID("Key".GetHashCode(), FocusType.Keyboard, pos);
                EditorUtility.DropDown(id, pos, Keys, Array.IndexOf(Keys, key.stringValue), selectedIndex => SetKeySerializedProperty(key, selectedIndex));
            };
        }

        private void SetSelectedAction(SerializedProperty sp, int index)
        {
            if (index != -1)
                sp.stringValue = _allActionTypes[index].AssemblyQualifiedName;
            serializedObject.ApplyModifiedProperties();
        }

        private void SetTagSerializedProperty(SerializedProperty sp, int index)
        {
            sp.stringValue = index != -1 ? Tags[index] : string.Empty;
            serializedObject.ApplyModifiedProperties();
        }

        private void SetKeySerializedProperty(SerializedProperty sp, int index)
        {
            sp.stringValue = index != -1 ? Keys[index] : string.Empty;
            serializedObject.ApplyModifiedProperties();
        }

        public static void NormalView()
        {
            StateTransitionInspector.ResetView();
        }


        private void SetupTransitions()
        {
            _transitionStates = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Transitions"), true, true, false, true);

            _transitionStates.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Transitons");
            };

            _transitionStates.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _transitionStates.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                SerializedProperty property = element.FindPropertyRelative("m_TargetState");
                string val = serializedObject.targetObject.name + "  ->  ";
                if (property != null && property.objectReferenceValue != null)
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), val + property.objectReferenceValue.name);
                else
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), val + "None");
            };
        }
    }
}
