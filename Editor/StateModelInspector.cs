using SAS.TagSystem;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ReorderableList = UnityEditorInternal.ReorderableList;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(StateModel))]
    public class StateModelInspector : UnityEditor.Editor
    {
        private ReorderableList _stateActions;
        private ReorderableList _transitionStates;

        private Type[] _allActionTypes;

        private StateTransitionInspector _stateTransitionInspector;
        private static TagList _tagList;

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
                target.name = curName;
                AssetDatabase.SaveAssets();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (_stateTransitionInspector == null || !_stateTransitionInspector.OnInspectorGUI())
            {
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
                EditorGUI.LabelField(rect, name);
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
                _tagList = (TagList)EditorGUI.ObjectField(new Rect(rect.width - 80, rect.y - 2, 120, rect.height), _tagList, typeof(TagList), false);
                var fullName = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("fullName");
                var tag = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("tag");
                rect.y += 2;
                var curActionIndex = Array.FindIndex(_allActionTypes, type => type.Name == fullName.stringValue);
                var newActionIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width - 260, EditorGUIUtility.singleLineHeight), curActionIndex, _allActionTypes.Select(type => SerializedType.Sanitize(type.ToString())).ToArray());
                if (newActionIndex != curActionIndex)
                    fullName.stringValue = _allActionTypes[newActionIndex].AssemblyQualifiedName;

                EditorGUI.LabelField(new Rect(rect.x + 5, rect.y - 2, rect.width -260, rect.height), SerializedType.Sanitize(fullName.stringValue));

                if (_tagList == null)
                {
                    var curTag = EditorGUI.DelayedTextField(new Rect(rect.width - 210, rect.y - 2, 120, rect.height), tag.stringValue);
                    if (curTag != tag.stringValue)
                        tag.stringValue = curTag;
                }
                else
                {
                    var tagIndex = EditorGUI.Popup(new Rect(rect.width - 210, rect.y - 2, 120, rect.height), Array.IndexOf(_tagList.tags, tag.stringValue), _tagList.tags);
                    if (tagIndex != -1)
                        tag.stringValue = _tagList.tags[tagIndex];

                    reorderableList.serializedProperty.serializedObject.ApplyModifiedProperties();
                }
            };
        }

        public static void NormalView()
        {
            StateTransitionInspector.ResetView();
        }


        private void SetupTransitions()
        {
            _transitionStates = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Transitions"), false, true, false, false);

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
