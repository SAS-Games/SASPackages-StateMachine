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

        public static void AddStateTransition(this StateModel sourceStateModel, RuntimeStateMachineController runtimeStateMachineController, StateModel targerStateModel)
        {
            var transitionStateModel = sourceStateModel.CreateStateTransitionModel(runtimeStateMachineController, targerStateModel);

            var transitionStateModelSO = new SerializedObject(transitionStateModel);
            var sourceState = transitionStateModelSO.FindProperty("m_SourceState");
            var targetState = transitionStateModelSO.FindProperty("m_TargetState");
            sourceState.objectReferenceValue = sourceStateModel;
            targetState.objectReferenceValue = targerStateModel;
            transitionStateModelSO.ApplyModifiedProperties();

            var conditions = transitionStateModelSO.FindProperty("m_Conditions");
            conditions.arraySize = 0;

            var stateTranstionsList = sourceStateModel.GetTransitionsProp();
            stateTranstionsList.InsertArrayElementAtIndex(stateTranstionsList.arraySize);
            var element = stateTranstionsList.GetArrayElementAtIndex(stateTranstionsList.arraySize - 1);
            element.objectReferenceValue = transitionStateModel;

            stateTranstionsList.serializedObject.ApplyModifiedProperties();
            sourceStateModel.ToSerializedObject().ApplyModifiedProperties();
        }

        private static StateTransitionModel CreateStateTransitionModel(this StateModel sourceStateModel, RuntimeStateMachineController runtimeStateMachineController, StateModel targerStateModel)
        {
            var stateTransitionModel = ScriptableObject.CreateInstance<StateTransitionModel>();
            stateTransitionModel.name = sourceStateModel.name + "->To->" + targerStateModel.name;

            if (AssetDatabase.GetAssetPath(runtimeStateMachineController) != "")
                AssetDatabase.AddObjectToAsset(stateTransitionModel, AssetDatabase.GetAssetPath(runtimeStateMachineController));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return stateTransitionModel;
        }

        internal static int GetTransitionStateIndex(this StateModel state, StateModel targetState)
        {
            var stateTransitions = state.GetTransitionsProp();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = (StateTransitionModel)stateTransitions.GetArrayElementAtIndex(i).objectReferenceValue;
                if (element.ToSerializedObject().FindProperty("m_TargetState").objectReferenceValue == targetState)
                    return i;
            }

            return -1;
        }

        internal static StateTransitionModel GetTransitionStateModel(this StateModel state, StateModel targetState)
        {
            var stateTransitions = state.GetTransitionsProp();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = (StateTransitionModel)stateTransitions.GetArrayElementAtIndex(i).objectReferenceValue;
                if (element.ToSerializedObject().FindProperty("m_TargetState").objectReferenceValue == targetState)
                    return element;
            }

            return null;
        }

        internal static int GetTransitionCount(this StateModel state, StateModel targetState)
        {
            int count = 0;
            var stateTransitions = state.GetTransitionsProp();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = (StateTransitionModel)stateTransitions.GetArrayElementAtIndex(i).objectReferenceValue;
                if (element != null)
                {
                    if (element.ToSerializedObject().FindProperty("m_TargetState").objectReferenceValue == targetState)
                        count++;
                }
            }

            return count;
        }

        internal static SerializedObject ToSerializedObject(this StateModel stateModel)
        {
            return new SerializedObject(stateModel);
        }

        internal static SerializedObject ToSerializedObject(this StateTransitionModel stateTransitionModel)
        {
            return new SerializedObject(stateTransitionModel);
        }

        /// <summary>
        /// clear all statetransions between source and target state. 
        /// if target state is null clear all the transtions from the source state model.
        /// also remove the transion state model assets
        /// </summary>
        /// <param name="sourceStateModel"></param>
        /// <param name="targetStateModel"></param>
        public static void ClearConnection(this StateModel sourceStateModel, StateModel targetStateModel = null)
        {
            var stateTransitions = sourceStateModel.GetTransitionsProp();
            List<StateTransitionModel> stateTransitionModelsToDelete = new List<StateTransitionModel>();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = ((StateTransitionModel)stateTransitions.GetArrayElementAtIndex(i).objectReferenceValue);
                if (targetStateModel == null || element.ToSerializedObject().FindProperty("m_TargetState").objectReferenceValue == targetStateModel)
                {
                    stateTransitions.DeleteArrayElementAtIndex(i);
                    stateTransitions.serializedObject.ApplyModifiedProperties();
                    if (stateTransitions.GetArrayElementAtIndex(i) != null)
                    {
                        stateTransitions.DeleteArrayElementAtIndex(i);
                        stateTransitions.serializedObject.ApplyModifiedProperties();
                    }

                    i--;
                    stateTransitionModelsToDelete.Add(element);
                    sourceStateModel.ToSerializedObject().ApplyModifiedProperties();
                }
            }

            DestroyImmediateStateTransitionModels(stateTransitionModelsToDelete.ToArray());
        }

        internal static void DestroyImmediateStateTransitionModels(StateTransitionModel[] stateTransitionModels)
        {
            for (int i = 0; i < stateTransitionModels.Length; ++i)
                stateTransitionModels[i].DestroyImmediate();
        }

        internal static void DestroyImmediate(this StateTransitionModel stateTransitionModel)
        {
            Object.DestroyImmediate(stateTransitionModel, true);
            AssetDatabase.SaveAssets();
        }

        internal static  void ResetTransitions(this StateModel stateModel)
        {
            var stateTransitions = stateModel.GetTransitionsProp();
            stateTransitions.arraySize = 0;
            stateTransitions.serializedObject.ApplyModifiedProperties();
            stateModel.ToSerializedObject().ApplyModifiedProperties();
        }

        internal static StateTransitionModel Clone(this StateTransitionModel stateTransitionModel, RuntimeStateMachineController runtimeStateMachineController, StateMachineModel toStateMachineModel)
        {
            var clonedStateTransitionModel = Object.Instantiate(stateTransitionModel);
            clonedStateTransitionModel.name = stateTransitionModel.name;
            if (AssetDatabase.GetAssetPath(runtimeStateMachineController) != "")
                AssetDatabase.AddObjectToAsset(clonedStateTransitionModel, AssetDatabase.GetAssetPath(runtimeStateMachineController));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var sourceStateName = stateTransitionModel.ToSerializedObject().FindProperty("m_SourceState").objectReferenceValue.name;
            var targetStateName = stateTransitionModel.ToSerializedObject().FindProperty("m_TargetState").objectReferenceValue.name;

            var transitionStateModelSO = clonedStateTransitionModel.ToSerializedObject();
            transitionStateModelSO.FindProperty("m_SourceState").objectReferenceValue = toStateMachineModel.GetStateModel(sourceStateName);
            transitionStateModelSO.FindProperty("m_TargetState").objectReferenceValue = toStateMachineModel.GetStateModel(targetStateName);
            transitionStateModelSO.ApplyModifiedProperties();

            var sourceStateModel = toStateMachineModel.GetStateModel(sourceStateName);
            var stateTranstionsList = sourceStateModel.GetTransitionsProp();
            stateTranstionsList.InsertArrayElementAtIndex(stateTranstionsList.arraySize);
            var element = stateTranstionsList.GetArrayElementAtIndex(stateTranstionsList.arraySize - 1);
            element.objectReferenceValue = clonedStateTransitionModel;

            stateTranstionsList.serializedObject.ApplyModifiedProperties();
            sourceStateModel.ToSerializedObject().ApplyModifiedProperties();

            return clonedStateTransitionModel;
        }
    }
}
