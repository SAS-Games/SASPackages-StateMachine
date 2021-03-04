using SAS.TagSystem;
using System;
using System.Collections.Generic;
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
            if (target.name.Equals(Util.AnyStateModelName))
                return;
            var curName = EditorGUI.DelayedTextField(new Rect(50, 30, EditorGUIUtility.currentViewWidth - 60, EditorGUIUtility.singleLineHeight), new GUIContent("State Name"), target.name);
            if (curName != target.name)
            {
                var mainAsset = new SerializedObject(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target)));
                var states = mainAsset.FindProperty("_stateModels");
                var usedName = new HashSet<string>();
                for (int i = 0; i < states.arraySize; ++i)
                    usedName.Add(states.GetArrayElementAtIndex(i).objectReferenceValue.name);
                curName = Util.MakeUniqueName(curName, usedName);

                target.name = curName;
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target), ImportAssetOptions.ForceUpdate);
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
                _tagList = (TagList)EditorGUI.ObjectField(new Rect(rect.width - 60, rect.y - 2, 100, rect.height), _tagList, typeof(TagList), false);
                var fullName = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("fullName");
                var tag = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("tag");
                var key = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("key");
                rect.y += 2;
                var curActionIndex = Array.FindIndex(_allActionTypes, type => type.Name == fullName.stringValue);
                var newActionIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width - 280, EditorGUIUtility.singleLineHeight), curActionIndex, _allActionTypes.Select(type => SerializedType.Sanitize(type.ToString())).ToArray());
                if (newActionIndex != curActionIndex)
                    fullName.stringValue = _allActionTypes[newActionIndex].AssemblyQualifiedName;

                EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, rect.width -260, rect.height), SerializedType.Sanitize(fullName.stringValue));
                var curKey = TextField(new Rect(rect.width - 130, rect.y - 2, 60, rect.height - 2), key.stringValue, "Key");
                if (curKey != key.stringValue)
                    key.stringValue = curKey;
                
                if (_tagList == null)
                {
                    var curTag = TextField(new Rect(rect.width - 230, rect.y -2, 90, rect.height -2 ), tag.stringValue, "Tag");
                    if (curTag != tag.stringValue)
                        tag.stringValue = curTag;
                }
                else
                {
                    var tagIndex = EditorGUI.Popup(new Rect(rect.width - 230, rect.y - 2, 90, rect.height -2), Array.IndexOf(_tagList.tags, tag.stringValue), _tagList.tags);
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
            _transitionStates = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Transitions"), true, true, false, false);

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

        string TextField(Rect rect, string text, string placeholder)
        {
            var newText = EditorGUI.TextField(rect, text);
            if (string.IsNullOrEmpty(text))
            {
                var guiColor = GUI.color;
                GUI.color = Color.grey;
                EditorGUI.LabelField(rect, placeholder);
                GUI.color = guiColor;
            }
            return newText;
        }
    }
}
