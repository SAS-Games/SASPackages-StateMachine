using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public static class StateModelExtensions
    {
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

        public static void AddStateTransition(this StateModel sourceStateModel, StateModel targerStateModel)
        {
            var stateModelSO = new SerializedObject(sourceStateModel);
            var stateTranstionsList = stateModelSO.FindProperty("m_Transitions");
            stateTranstionsList.InsertArrayElementAtIndex(stateTranstionsList.arraySize);

            var transitionState = stateTranstionsList.GetArrayElementAtIndex(stateTranstionsList.arraySize - 1);
            var targetState = transitionState.FindPropertyRelative("m_TargetState");
            targetState.objectReferenceValue = targerStateModel;

            var conditions = transitionState.FindPropertyRelative("m_Conditions");
            conditions.arraySize = 0;
            stateTranstionsList.serializedObject.ApplyModifiedProperties();
            stateModelSO.ApplyModifiedProperties();
        }
    }
}
