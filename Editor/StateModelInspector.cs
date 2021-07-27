using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ReorderableList = UnityEditorInternal.ReorderableList;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;
using SAS.TagSystem.Editor;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(StateModel))]
    public class StateModelInspector : UnityEditor.Editor
    {
        private ReorderableList _stateActions;
        private ReorderableList _transitionStates;
        private Type[] _allActionTypes;

        private static string[] Tags => TagList.GetList();
        private static string[] Keys => TagList.GetList("Key List");
        private GUIStyle _actionNotFoundStyle = new GUIStyle();
        new private SerializedObject serializedObject;

        private void OnEnable()
        {
            serializedObject = ((StateModel)target).serializedObject();
            _actionNotFoundStyle.normal.textColor = Color.red;
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
            if (!target.name.Equals(Util.AnyStateModelName))
                _stateActions.DoLayoutList();
            _transitionStates.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
            Repaint();
        }

        private void HandleReorderableActionsList(ReorderableList reorderableList, string name)
        {
            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                EditorGUI.LabelField(rect, name);
                rect.width -= 120;
                var width = 100;

                var pos = new Rect(rect.width - Mathf.Min(100, rect.width / 3 - 20) - 20, rect.y, width, rect.height);
                EditorGUI.LabelField(pos, "Tag", style);
                pos = new Rect(rect.width - Mathf.Min(40, rect.width / 3 - 40), rect.y, width, rect.height);
                EditorGUI.LabelField(pos, "Key", style);

                pos = new Rect((rect.width + 120) - Mathf.Min(70, rect.width / 3 - 10), rect.y, width, rect.height);
                EditorGUI.LabelField(pos, "WhenToExecute", style);
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
                var whenToExecute = reorderableList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("whenToExecute");

                if (GUI.Button(new Rect(rect.x, rect.y, 30, rect.height - 5), "C#"))
                {
                    var assetsPath = AssetDatabase.GetAllAssetPaths();
                    foreach (var path in assetsPath)
                    {
                        var script = (MonoScript)AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript));
                        if (script != null)
                        {
                            if (script.GetClass()?.AssemblyQualifiedName == actionFullName.stringValue)
                                AssetDatabase.OpenAsset(script);
                        }
                    }
                }

                rect.y += 2;
                var curActionIndex = Array.FindIndex(_allActionTypes, type => type.AssemblyQualifiedName == actionFullName.stringValue);
                var pos = new Rect(rect.x + 30, rect.y - 2, Mathf.Max(rect.width - 280, 40), rect.height - 2);
                int id = GUIUtility.GetControlID("actionFullName".GetHashCode(), FocusType.Keyboard, pos);
                if (curActionIndex != -1 || string.IsNullOrEmpty(actionFullName.stringValue))
                    EditorUtility.DropDown(id, pos, _allActionTypes.Select(type => SerializedType.Sanitize(type.ToString())).ToArray(), curActionIndex, selectedIndex => SetSelectedAction(actionFullName, selectedIndex));
                else
                    EditorUtility.DropDown(id, pos, _allActionTypes.Select(type => SerializedType.Sanitize(type.ToString())).ToArray(), curActionIndex, actionFullName.stringValue, Color.red, selectedIndex => SetSelectedAction(actionFullName, selectedIndex));

                var width = Mathf.Min(80, rect.width / 5);
                var rectEnd = rect.width - 2.5f * width;
                pos = new Rect(rectEnd, rect.y - 2, width, rect.height - 2);
                id = GUIUtility.GetControlID("Tag".GetHashCode(), FocusType.Keyboard, pos);
                var tagIndex = Array.IndexOf(Tags, tag.stringValue);
                if (tagIndex != -1 || string.IsNullOrEmpty(tag.stringValue))
                    EditorUtility.DropDown(id, pos, Tags, tagIndex, selectedIndex => SetTagSerializedProperty(tag, selectedIndex));
                else
                    EditorUtility.DropDown(id, pos, Tags, tagIndex, tag.stringValue, Color.red, selectedIndex => SetTagSerializedProperty(tag, selectedIndex));

                rectEnd = rect.width - 1.5f * width;
                pos = new Rect(rectEnd, rect.y - 2, width, rect.height);
                id = GUIUtility.GetControlID("Key".GetHashCode(), FocusType.Keyboard, pos);
                var keyIndex = Array.IndexOf(Keys, key.stringValue);
                if (keyIndex != -1 || string.IsNullOrEmpty(key.stringValue))
                    EditorUtility.DropDown(id, pos, Keys, keyIndex, selectedIndex => SetKeySerializedProperty(key, selectedIndex));
                else
                    EditorUtility.DropDown(id, pos, Keys, keyIndex, key.stringValue, Color.red, selectedIndex => SetKeySerializedProperty(key, selectedIndex));

                rectEnd = rect.width - width / 2;
                pos = new Rect(rectEnd, rect.y - 2, width, rect.height);
                uint a = (uint)(EditorGUI.MaskField(pos, "", whenToExecute.intValue, whenToExecute.enumNames));
                if (EditorGUI.EndChangeCheck())
                    whenToExecute.intValue = (int)a;
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

        private void SetupTransitions()
        {
            _transitionStates = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Transitions"), true, true, false, true);

            _transitionStates.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Transitons");
            };

            _transitionStates.onRemoveCallback = list =>
            {
                var allTranstionFromThisState = _transitionStates.serializedProperty;
                var selectedStateTransitionModel = (StateTransitionModel)allTranstionFromThisState.GetArrayElementAtIndex(list.index).objectReferenceValue;


                allTranstionFromThisState.DeleteArrayElementAtIndex(list.index);
                allTranstionFromThisState.serializedObject.ApplyModifiedProperties();
                if (allTranstionFromThisState.GetArrayElementAtIndex(list.index) != null)
                {
                    allTranstionFromThisState.DeleteArrayElementAtIndex(list.index);
                    allTranstionFromThisState.serializedObject.ApplyModifiedProperties();
                }

                serializedObject.ApplyModifiedProperties();
                selectedStateTransitionModel.DestroyImmediate();
            };
          

            _transitionStates.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = ((StateTransitionModel)_transitionStates.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue).serializedObject();
                rect.y += 2;
                SerializedProperty property = element.FindProperty("m_TargetState");
                string val = serializedObject.targetObject.name + "  ->  ";
                if (property != null && property.objectReferenceValue != null)
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), val + property.objectReferenceValue.name);
                else
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), val + "None");
            };
        }
    }
}
