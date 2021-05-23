using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public static class StateModelExtensions
    {
        const string TransitionsVar = "m_Transitions";
        public static void SetPosition(this StateModel stateModel, Vector3 position)
        {
            var stateModelSO = new SerializedObject(stateModel);
            stateModelSO.FindProperty("m_Position").vector3Value = position;
            stateModelSO.ApplyModifiedProperties();
        }

        public static Vector3 GetPosition(this StateModel stateModel)
        {
            var stateModelSO = new SerializedObject(stateModel);
            return stateModelSO.FindProperty("m_Position").vector3Value;
        }

        public static SerializedProperty GetTransitionsProp(this StateModel stateModel)
        {
            return new SerializedObject(stateModel).FindProperty(TransitionsVar);
        }

        public static void AddStateTransition(this StateModel sourceStateModel, StateModel targerStateModel)
        {
            var stateModelSO = new SerializedObject(sourceStateModel);
            var stateTranstionsList = stateModelSO.FindProperty(TransitionsVar);
            stateTranstionsList.InsertArrayElementAtIndex(stateTranstionsList.arraySize);

            var transitionState = stateTranstionsList.GetArrayElementAtIndex(stateTranstionsList.arraySize - 1);
            var targetState = transitionState.FindPropertyRelative("m_TargetState");
            targetState.objectReferenceValue = targerStateModel;

            var conditions = transitionState.FindPropertyRelative("m_Conditions");
            conditions.arraySize = 0;
            stateTranstionsList.serializedObject.ApplyModifiedProperties();
            stateModelSO.ApplyModifiedProperties();
        }

        public static int GetTransitionStateIndex(this StateModel state, StateModel targetState)
        {
            var stateTransitions = state.GetTransitionsProp();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = stateTransitions.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("m_TargetState").objectReferenceValue == targetState)
                    return i;
            }

            return -1;
        }

        public static int GetTransitionCount(this StateModel state, StateModel targetState)
        {
            int count = 0;
            var stateTransitions = state.GetTransitionsProp();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = stateTransitions.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("m_TargetState").objectReferenceValue == targetState)
                    count++;
            }

            return count;
        }

        public static SerializedObject ToSerializedObject(this StateModel stateModel)
        {
            return new SerializedObject(stateModel);
        }

        public static void ClearConnection(this StateModel sourceStateModel, StateModel targetStateMode)
        {
            var stateTransitions = sourceStateModel.GetTransitionsProp();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = stateTransitions.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("m_TargetState").objectReferenceValue == targetStateMode)
                {
                    stateTransitions.DeleteArrayElementAtIndex(i);
                    stateTransitions.serializedObject.ApplyModifiedProperties();
                    break;
                }

            }
        }
    }
}
