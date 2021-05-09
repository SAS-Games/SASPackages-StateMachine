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
            stateModelSO.FindProperty("position").vector3Value = position;
            stateModelSO.ApplyModifiedProperties();
        }

        public static Vector3 GetPosition(this StateModel stateModel)
        {
            var stateModelSO = new SerializedObject(stateModel);
            return stateModelSO.FindProperty("position").vector3Value;
        }
    }
}
