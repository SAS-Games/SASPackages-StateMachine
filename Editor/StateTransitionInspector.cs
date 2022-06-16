using SAS.StateMachineGraph.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;

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

        private  string[] _parametersList;
        private ReorderableList _allTranstionsToTargetState;
        private ReorderableList _transitionConditions;
       
        private SerializedObject _stateMachineSO;
        private SerializedObject _stateTransitionModelSO;
        private SerializedObject _sourceStateModelSO;

        private int _transitionCount;
        public static int SelectedTransitionIndex = -1;
        private Type[] _allCustomtriggerTypes;

        private void OnEnable()
        {
            _allCustomtriggerTypes = AppDomain.CurrentDomain.GetAllDerivedTypes<ICustomTrigger>().ToArray();
            _stateTransitionModelSO = new SerializedObject(target);
            var runtimeStateMachineController = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target)) as RuntimeStateMachineController;
            _transitionConditions = new ReorderableList(_stateTransitionModelSO, _stateTransitionModelSO.FindProperty("m_Conditions"), true, true, true, true);

            var transitionStateModelSO = ((StateTransitionModel)target).serializedObject();
            var sourceState = ((StateModel)transitionStateModelSO.FindProperty("m_SourceState").objectReferenceValue);
            var targetState = ((StateModel)transitionStateModelSO.FindProperty("m_TargetState").objectReferenceValue);
            _sourceStateModelSO = new SerializedObject(sourceState);
            _transitionCount = sourceState.GetTransitionCount(targetState);
            DrawConditionBlock();
            Show(runtimeStateMachineController, sourceState.serializedObject());
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
            _allTranstionsToTargetState?.DoLayoutList();

            var hasExitTime = _stateTransitionModelSO.FindProperty("m_HasExitTime");
            var exitTime = _stateTransitionModelSO.FindProperty("m_ExitTime");

            hasExitTime.boolValue = EditorGUILayout.Toggle("Has Exit Time", hasExitTime.boolValue);

            EditorGUI.BeginDisabledGroup(hasExitTime.boolValue == false);
            exitTime.floatValue = EditorGUILayout.FloatField("Exit Time", exitTime.floatValue);
            EditorGUI.EndDisabledGroup();
            
            var waitForAwaitableActionsToComplete = _stateTransitionModelSO.FindProperty("m_WaitForAwaitableActionsToComplete");
            waitForAwaitableActionsToComplete.boolValue = EditorGUILayout.Toggle("Wait For Awaitable Actions To Complete", waitForAwaitableActionsToComplete.boolValue);
            _stateTransitionModelSO.ApplyModifiedProperties();

            EditorGUILayout.Space(10);
            _transitionConditions.DoLayoutList();
        }

        private ReorderableList DrawConditionBlock()
        {
            _transitionConditions.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Conditions");
            };

            _transitionConditions.onAddCallback = list =>
            {
                _transitionConditions.serializedProperty.InsertArrayElementAtIndex(_transitionConditions.serializedProperty.arraySize);
                var element = _transitionConditions.serializedProperty.GetArrayElementAtIndex(_transitionConditions.serializedProperty.arraySize - 1);
              
                SerializedProperty defaultName = element.FindPropertyRelative("m_Name");
                var defaultMode = element.FindPropertyRelative("m_Mode");
                var defaultFloat = element.FindPropertyRelative("m_FloatValue");

                if (_parametersList != null && _parametersList.Length > 0)
                    defaultName.stringValue = _parametersList[0];
                defaultMode.intValue = -1;
                defaultFloat.floatValue = 0;

                _transitionConditions.serializedProperty.serializedObject.ApplyModifiedProperties();
                ApplyConditionModifiedProperties();
            };

            _transitionConditions.onRemoveCallback = list =>
            {
                _transitionConditions.serializedProperty.DeleteArrayElementAtIndex(list.index);
                _transitionConditions.serializedProperty.serializedObject.ApplyModifiedProperties();
                ApplyConditionModifiedProperties();
            };

            _transitionConditions.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _transitionConditions.serializedProperty.GetArrayElementAtIndex(index);
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

                if (type.intValue == 0) //ParameterType.CustomTrigger
                {
                    var customtrigger = element.FindPropertyRelative("m_CustomTrigger");
                    var curActionIndex = Array.FindIndex(_allCustomtriggerTypes, ele => ele.AssemblyQualifiedName == customtrigger.stringValue);
                    var pos = new Rect(rect.width / 3 + 50, rect.y, rect.width - (rect.width / 3 + 20), rect.height);
                    int id = GUIUtility.GetControlID("customtrigger".GetHashCode(), FocusType.Keyboard, pos);
                    if (curActionIndex != -1 || string.IsNullOrEmpty(customtrigger.stringValue))
                        EditorUtility.DropDown(id, pos, _allCustomtriggerTypes.Select(ele => SerializedType.Sanitize(ele.ToString())).ToArray(), curActionIndex, selectedIndex => SetSelectedCustomTrigger(customtrigger, selectedIndex));
                    else
                        EditorUtility.DropDown(id, pos, _allCustomtriggerTypes.Select(ele => SerializedType.Sanitize(ele.ToString())).ToArray(), curActionIndex, customtrigger.stringValue, Color.red, selectedIndex => SetSelectedCustomTrigger(customtrigger, selectedIndex));
                }

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

                _transitionConditions.serializedProperty.serializedObject.ApplyModifiedProperties();
                ApplyConditionModifiedProperties();
            };
            return _transitionConditions;
        }

        private void Show(RuntimeStateMachineController runtimeStateMachineController, SerializedObject stateModelSO)
        {
            if (SelectedTransitionIndex != -1)
            {
                _stateMachineSO = runtimeStateMachineController.ToSerializedObject();
                FilterTransitions(SelectedTransitionIndex, stateModelSO);
                _parametersList = runtimeStateMachineController.ParametersName();
                _parametersList = _parametersList.Add("Custom");
            }
        }

        private int GetParameterType(int index)
        {
            var parameters = _stateMachineSO.FindProperty("_parameters");
            try
            {
                if (parameters.arraySize > 0)
                {
                    var element = parameters.GetArrayElementAtIndex(index);
                    var type = element.FindPropertyRelative("m_Type");
                    return type.intValue;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private int GetParameterIndex(string name)
        {
            return System.Array.IndexOf(_parametersList, name);
        }

        private void FilterTransitions(int index, SerializedObject stateModelSO)
        {
            var allTranstionFromThisState = stateModelSO.FindProperty("m_Transitions");
            if (allTranstionFromThisState?.arraySize > 0)
            {
                var element =allTranstionFromThisState.GetArrayElementAtIndex(index);
                var elementSO = ((StateTransitionModel)element.objectReferenceValue).serializedObject();
                var targetState = elementSO.FindProperty("m_TargetState").objectReferenceValue;
                var allTranstionsToTargetState = new List<SerializedProperty>();

                for (int i = 0; i < allTranstionFromThisState.arraySize; ++i)
                {
                    element = allTranstionFromThisState.GetArrayElementAtIndex(i);
                    elementSO = ((StateTransitionModel)element.objectReferenceValue).serializedObject();
                    var state = elementSO.FindProperty("m_TargetState").objectReferenceValue;
                    if (targetState == state)
                        allTranstionsToTargetState.Add(element);
                }

                SetupTransitions(allTranstionsToTargetState, stateModelSO);
            }
        }

        private void SetupTransitions(List<SerializedProperty> allTranstionsToTargetState, SerializedObject stateModelSO)
        {
            _allTranstionsToTargetState = new ReorderableList(allTranstionsToTargetState, typeof(SerializedProperty), false, true, false, true);

            _allTranstionsToTargetState.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Transitons");
            };

            _allTranstionsToTargetState.onSelectCallback = list =>
            {
                var stateTransitionModel = (StateTransitionModel)allTranstionsToTargetState[list.index].objectReferenceValue;
                SelectedTransitionIndex = list.index;
                Selection.activeObject = stateTransitionModel;
            };

            _allTranstionsToTargetState.onRemoveCallback = list =>
            {
                var selectedStateTransitionModel = (StateTransitionModel)allTranstionsToTargetState[list.index].objectReferenceValue;
                allTranstionsToTargetState.RemoveAt(list.index);
                SelectedTransitionIndex = list.index - 1;
                var allTranstionFromThisState = stateModelSO.FindProperty("m_Transitions");
               
                for (int i = 0; i < allTranstionFromThisState.arraySize; ++i)
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

                if (SelectedTransitionIndex > 0)
                    Selection.activeObject = (StateTransitionModel)allTranstionsToTargetState[list.index - 1].objectReferenceValue;
                else
                    Selection.activeObject = null;
            };

            _allTranstionsToTargetState.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = ((StateTransitionModel)allTranstionsToTargetState[index].objectReferenceValue).serializedObject();
                SerializedProperty property = element.FindProperty("m_TargetState");
                string val = stateModelSO.targetObject.name + "  ->  " + property.objectReferenceValue.name;
                rect.y += 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), val);
            };
        }

        private void ApplyConditionModifiedProperties()
        {
            _stateTransitionModelSO.ApplyModifiedProperties();
            _sourceStateModelSO.ApplyModifiedProperties();
        }

        private void SetSelectedCustomTrigger(SerializedProperty sp, int index)
        {
            if (index != -1)
                sp.stringValue = _allCustomtriggerTypes[index].AssemblyQualifiedName;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
