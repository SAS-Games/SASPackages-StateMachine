﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(StateTransitionModel))]
    public class StateTransitionInspector : UnityEditor.Editor
    {
        enum FloatMode
        {
            Greater = 3,
            Less = 4,
        }

        enum IntMode
        {
            Greater = 3,
            Less = 4,
            Equals = 5,
            NotEqual = 6
        }
        enum BoolMode
        {
            True = 1,
            False = 2
        }

        private static ReorderableList[] _transitionsConditionList;
        private static string[] _parametersList;
        public static int SelectedTransitionIndex = -1;
        private static SerializedObject _stateMachineSO;
        private static ReorderableList _allTranstionsToTargetState;
        private int _transitionCount;

        private void OnEnable()
        {
            var runtimeStateMachineController = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target)) as RuntimeStateMachineController;

            var transitionStateModelSO = ((StateTransitionModel)target).ToSerializedObject();
            var sourceState = ((StateModel)transitionStateModelSO.FindProperty("m_SourceState").objectReferenceValue);
            var targetState = ((StateModel)transitionStateModelSO.FindProperty("m_TargetState").objectReferenceValue);
            _transitionCount = sourceState.GetTransitionCount(targetState);
            Show(runtimeStateMachineController, sourceState.ToSerializedObject());
        }

        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();
            string stateTransition = "1 ActorTranstionBase";
            if (_transitionCount > 1)
                stateTransition = $"{_transitionCount} Transitions";
            EditorGUI.LabelField(new Rect(50, 30, EditorGUIUtility.currentViewWidth - 60, EditorGUIUtility.singleLineHeight), new GUIContent(stateTransition));
        }

        public override void OnInspectorGUI()
        {
            if (SelectedTransitionIndex == -1)
                return;

            _allTranstionsToTargetState.DoLayoutList();
            if (SelectedTransitionIndex != -1 && _allTranstionsToTargetState.list.Count > 0 && _allTranstionsToTargetState.list.Count > SelectedTransitionIndex)
            {
                var stateTransitionProp = ((StateTransitionModel)(_allTranstionsToTargetState.list[SelectedTransitionIndex] as SerializedProperty).objectReferenceValue).ToSerializedObject();
                var hasExitTime = stateTransitionProp.FindProperty("m_HasExitTime");
                var exitTime = stateTransitionProp.FindProperty("m_ExitTime");

                hasExitTime.boolValue = EditorGUILayout.Toggle("Has Exit Time", hasExitTime.boolValue);

                EditorGUI.BeginDisabledGroup(hasExitTime.boolValue == false);
                exitTime.floatValue = EditorGUILayout.FloatField("Exit Time", exitTime.floatValue);
                EditorGUI.EndDisabledGroup();
                stateTransitionProp.ApplyModifiedProperties();
            }
            EditorGUILayout.Space(10);

            for (int i = 0; i < _transitionsConditionList?.Length; ++i)
            {
                if (SelectedTransitionIndex != -1 && SelectedTransitionIndex == i)
                {
                    if (_transitionsConditionList[i] != null)
                        _transitionsConditionList[i].DoLayoutList();
                }
            }
        }

        private static ReorderableList DrawConditionBlock(SerializedProperty transitionState, SerializedObject serializedObject)
        {
            ReorderableList conditions = new ReorderableList(serializedObject, ((StateTransitionModel)transitionState.objectReferenceValue).ToSerializedObject().FindProperty("m_Conditions"), true, true, true, true);
            conditions.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Conditions");
            };

            conditions.onAddCallback = list =>
            {
                conditions.serializedProperty.InsertArrayElementAtIndex(conditions.serializedProperty.arraySize);
                var element = conditions.serializedProperty.GetArrayElementAtIndex(conditions.serializedProperty.arraySize - 1);
              
                SerializedProperty defaultName = element.FindPropertyRelative("m_Name");
                var defaultMode = element.FindPropertyRelative("m_Mode");
                var defaultFloat = element.FindPropertyRelative("m_FloatValue");

                if (_parametersList != null && _parametersList.Length > 0)
                    defaultName.stringValue = _parametersList[0];
                defaultMode.intValue = -1;
                defaultFloat.floatValue = 0;
                serializedObject.ApplyModifiedProperties();
            };

            conditions.onRemoveCallback = list =>
            {
                conditions.serializedProperty.DeleteArrayElementAtIndex(list.index);
                conditions.serializedProperty.serializedObject.ApplyModifiedProperties();
            };

            conditions.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = conditions.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                if (_parametersList == null || _parametersList.Length == 0)
                {
                    EditorGUI.LabelField(rect, "Parameter does not exist in Controller", GUI.skin.GetStyle("Font"));
                    return;
                }

                SerializedProperty name = element.FindPropertyRelative("m_Name");
                SerializedProperty mode = element.FindPropertyRelative("m_Mode");
                SerializedProperty type = element.FindPropertyRelative("m_Type");

                var curIndex = GetParameterIndex(name.stringValue);
                if (curIndex == -1)
                    curIndex = 0;
                curIndex = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width / 3, rect.height), curIndex, _parametersList);

                name.stringValue = _parametersList[curIndex];
                type.intValue = GetParameterType(curIndex);
                var value = element.FindPropertyRelative("m_FloatValue");

                if (type.intValue == 1) //ParameterType.Float
                {
                    if (!System.Enum.IsDefined(typeof(FloatMode), mode.intValue))
                        mode.intValue = (int)FloatMode.Greater;
                    mode.intValue = (int)(Condition.Mode)EditorGUI.EnumPopup(new Rect(rect.width / 3 + 50, rect.y, rect.width / 4, rect.height), (FloatMode)mode.intValue);
                    value.floatValue = EditorGUI.FloatField(new Rect(rect.width / 3 + rect.width / 4 + 60, rect.y, rect.width - (rect.width / 3 + rect.width / 4 + 25), rect.height), value.floatValue);
                }

                if (type.intValue == 3) //ParameterType.Int
                {
                    if (!System.Enum.IsDefined(typeof(IntMode), mode.intValue))
                        mode.intValue = (int)IntMode.Greater;
                    mode.intValue = (int)(Condition.Mode)EditorGUI.EnumPopup(new Rect(rect.width / 3 + 50, rect.y, rect.width / 4, rect.height), (IntMode)mode.intValue);
                    value.floatValue = EditorGUI.IntField(new Rect(rect.width / 3 + rect.width / 4 + 60, rect.y, rect.width - (rect.width / 3 + rect.width / 4 + 25), rect.height), (int)value.floatValue);
                }

                if (type.intValue == 4) //ParameterType.Bool
                {
                    if (!System.Enum.IsDefined(typeof(BoolMode), mode.intValue))
                        mode.intValue = (int)BoolMode.True;
                    mode.intValue = (int)(Condition.Mode)EditorGUI.EnumPopup(new Rect(rect.width / 3 + 50, rect.y, rect.width - (rect.width / 3 + 15), rect.height), (BoolMode)mode.intValue);
                }
                serializedObject.ApplyModifiedProperties();
            };
            return conditions;
        }

        private static void Show(RuntimeStateMachineController runtimeStateMachineController, SerializedObject stateModelSO)
        {
            if (SelectedTransitionIndex != -1)
            {
                _stateMachineSO = runtimeStateMachineController.ToSerializedObject();
                FilterTransitions(SelectedTransitionIndex, stateModelSO);
                _parametersList = runtimeStateMachineController.ParametersName();
            }
        }

        private static int GetParameterType(int index)
        {
            var parameters = _stateMachineSO.FindProperty("_parameters");
            var element = parameters.GetArrayElementAtIndex(index);
            var type = element.FindPropertyRelative("m_Type");
            return type.intValue;
        }

        private static int GetParameterIndex(string name)
        {
            return System.Array.IndexOf(_parametersList, name);
        }

        private static void FilterTransitions(int index, SerializedObject stateModelSO)
        {
            var allTranstionFromThisState = stateModelSO.FindProperty("m_Transitions");
            if (allTranstionFromThisState?.arraySize > 0)
            {
                var element =allTranstionFromThisState.GetArrayElementAtIndex(index);
                var elementSO = ((StateTransitionModel)element.objectReferenceValue).ToSerializedObject();
                var targetState = elementSO.FindProperty("m_TargetState").objectReferenceValue;
                var allTranstionsToTargetState = new List<SerializedProperty>();

                for (int i = 0; i < allTranstionFromThisState.arraySize; ++i)
                {
                    element = allTranstionFromThisState.GetArrayElementAtIndex(i);
                    elementSO = ((StateTransitionModel)element.objectReferenceValue).ToSerializedObject();
                    var state = elementSO.FindProperty("m_TargetState").objectReferenceValue;
                    if (targetState == state)
                        allTranstionsToTargetState.Add(element);
                }

                SetupTransitions(allTranstionsToTargetState, stateModelSO);
            }
        }

        private static void SetupTransitions(List<SerializedProperty> allTranstionsToTargetState, SerializedObject stateModelSO)
        {
            _allTranstionsToTargetState = new ReorderableList(allTranstionsToTargetState, typeof(SerializedProperty), false, true, false, true);

            if (allTranstionsToTargetState?.Count > 0)
            {
                _transitionsConditionList = new ReorderableList[allTranstionsToTargetState.Count];
                for (int i = 0; i < _transitionsConditionList.Length; ++i)
                    _transitionsConditionList[i] = DrawConditionBlock(allTranstionsToTargetState[i], stateModelSO);
            }

            _allTranstionsToTargetState.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Transitons");
            };

            _allTranstionsToTargetState.onSelectCallback = list =>
            {
                SelectedTransitionIndex = list.index;
            };

            _allTranstionsToTargetState.onRemoveCallback = list =>
            {
                var selectedStateTransitionModel = (StateTransitionModel)allTranstionsToTargetState[list.index].objectReferenceValue;
                allTranstionsToTargetState.RemoveAt(list.index);
                SelectedTransitionIndex = list.index - 1;
                var allTranstionFromThisState = stateModelSO.FindProperty("m_Transitions");
                int i = 0;
                for (; i < allTranstionFromThisState.arraySize; ++i)
                {
                    var element = allTranstionFromThisState.GetArrayElementAtIndex(i).objectReferenceValue;

                    var stateTransitionModel = ((StateTransitionModel)allTranstionFromThisState.GetArrayElementAtIndex(i).objectReferenceValue);
                    if (stateTransitionModel == selectedStateTransitionModel)
                    {
                        allTranstionFromThisState.DeleteArrayElementAtIndex(i);
                        allTranstionFromThisState.serializedObject.ApplyModifiedProperties();
                        if (allTranstionFromThisState.GetArrayElementAtIndex(i) != null)
                        {
                            allTranstionFromThisState.DeleteArrayElementAtIndex(i);
                            allTranstionFromThisState.serializedObject.ApplyModifiedProperties();
                        }

                        stateModelSO.ApplyModifiedProperties();
                        selectedStateTransitionModel.DestroyImmediate();
                        break;
                    }
                }
            };

            _allTranstionsToTargetState.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = ((StateTransitionModel)allTranstionsToTargetState[index].objectReferenceValue).ToSerializedObject();
                SerializedProperty property = element.FindProperty("m_TargetState");
                string val = stateModelSO.targetObject.name + "  ->  " + property.objectReferenceValue.name;
                rect.y += 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), val);
            };
        }
    }
}
