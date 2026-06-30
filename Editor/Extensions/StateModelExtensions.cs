using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public static class StateModelExtensions
    {
        const string TransitionsVar = "m_Transitions";
        const string SourceNodeVar = "m_SourceNode";
        const string TargetNodeVar = "m_TargetNode";
        const string SourceStateVar = "m_SourceState";
        const string TargetStateVar = "m_TargetState";
        public static void SetPosition(this StateModel stateModel, Vector3Int position)
        {
            var stateModelSO = new SerializedObject(stateModel);
            stateModelSO.FindProperty("m_Position").vector3IntValue = position;
            stateModelSO.ApplyModifiedProperties();
        }

        public static Vector3Int GetPosition(this StateModel stateModel)
        {
            var stateModelSO = new SerializedObject(stateModel);
            return stateModelSO.FindProperty("m_Position").vector3IntValue;
        }

        public static SerializedProperty GetTransitionsProp(this TransitionNodeModel stateModel)
        {
            return new SerializedObject(stateModel).FindProperty(TransitionsVar);
        }

        public static void AddStateTransition(this TransitionNodeModel sourceStateModel, RuntimeStateMachineController runtimeStateMachineController, TransitionNodeModel targerStateModel)
        {
            if (!runtimeStateMachineController.IsAssetBacked() || sourceStateModel == null || targerStateModel == null)
                return;

            var transitionStateModel = sourceStateModel.CreateStateTransitionModel(runtimeStateMachineController, targerStateModel);
            if (transitionStateModel == null)
                return;

            var transitionStateModelSO = new SerializedObject(transitionStateModel);
            var sourceNode = transitionStateModelSO.FindProperty(SourceNodeVar);
            var targetNode = transitionStateModelSO.FindProperty(TargetNodeVar);
            var sourceState = transitionStateModelSO.FindProperty(SourceStateVar);
            var targetState = transitionStateModelSO.FindProperty(TargetStateVar);
            sourceNode.objectReferenceValue = sourceStateModel;
            targetNode.objectReferenceValue = targerStateModel;
            sourceState.objectReferenceValue = sourceStateModel as StateModel;
            targetState.objectReferenceValue = targerStateModel as StateModel;
            transitionStateModelSO.ApplyModifiedProperties();

            var conditions = transitionStateModelSO.FindProperty("m_Conditions");
            conditions.arraySize = 0;

            var stateTranstionsList = sourceStateModel.GetTransitionsProp();
            stateTranstionsList.InsertArrayElementAtIndex(stateTranstionsList.arraySize);
            var element = stateTranstionsList.GetArrayElementAtIndex(stateTranstionsList.arraySize - 1);
            element.objectReferenceValue = transitionStateModel;
            stateTranstionsList.serializedObject.ApplyModifiedProperties();
            sourceStateModel.serializedObject().ApplyModifiedProperties();
        }

        private static StateTransitionModel CreateStateTransitionModel(this TransitionNodeModel sourceStateModel, RuntimeStateMachineController runtimeStateMachineController, TransitionNodeModel targerStateModel)
        {
            if (!runtimeStateMachineController.IsAssetBacked())
                return null;

            var stateTransitionModel = ScriptableObject.CreateInstance<StateTransitionModel>();
            stateTransitionModel.name = sourceStateModel.name + "->To->" + targerStateModel.name;
            runtimeStateMachineController.AddObjectToAsset(stateTransitionModel);

            return stateTransitionModel;
        }

        internal static int GetTransitionStateIndex(this TransitionNodeModel state, TransitionNodeModel targetState)
        {
            var stateTransitions = state.GetTransitionsProp();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = stateTransitions.GetArrayElementAtIndex(i).objectReferenceValue as StateTransitionModel;
                if (element != null && element.TargetNodeModel == targetState)
                    return i;
            }

            return -1;
        }

        internal static StateTransitionModel GetTransitionStateModel(this TransitionNodeModel state, TransitionNodeModel targetState)
        {
            var stateTransitions = state.GetTransitionsProp();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = stateTransitions.GetArrayElementAtIndex(i).objectReferenceValue as StateTransitionModel;
                if (element != null && element.TargetNodeModel == targetState)
                    return element;
            }

            return null;
        }

        internal static int GetTransitionCount(this TransitionNodeModel state, TransitionNodeModel targetState)
        {
            int count = 0;
            var stateTransitions = state.GetTransitionsProp();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = stateTransitions.GetArrayElementAtIndex(i).objectReferenceValue as StateTransitionModel;
                if (element != null)
                {
                    if (element.TargetNodeModel == targetState)
                        count++;
                }
            }

            return count;
        }

        internal static SerializedObject serializedObject(this StateModel stateModel)
        {
            return new SerializedObject(stateModel);
        }

        internal static SerializedObject serializedObject(this TransitionNodeModel stateModel)
        {
            return new SerializedObject(stateModel);
        }

        internal static SerializedObject serializedObject(this StateTransitionModel stateTransitionModel)
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
        public static void ClearConnection(this TransitionNodeModel sourceStateModel, TransitionNodeModel targetStateModel = null)
        {
            var stateTransitions = sourceStateModel.GetTransitionsProp();
            List<StateTransitionModel> stateTransitionModelsToDelete = new List<StateTransitionModel>();
            for (int i = 0; i < stateTransitions.arraySize; ++i)
            {
                var element = stateTransitions.GetArrayElementAtIndex(i).objectReferenceValue as StateTransitionModel;
                if (element == null || targetStateModel == null || element.TargetNodeModel == targetStateModel)
                {
                    stateTransitions.DeleteArrayElementAtIndex(i);
                    stateTransitions.serializedObject.ApplyModifiedProperties();
                    i--;
                    if (element != null)
                        stateTransitionModelsToDelete.Add(element);
                    sourceStateModel.serializedObject().ApplyModifiedProperties();
                }
            }

            DestroyImmediateStateTransitionModels(stateTransitionModelsToDelete.ToArray());
        }

        internal static void DestroyImmediateStateTransitionModels(StateTransitionModel[] stateTransitionModels)
        {
            for (int i = 0; i < stateTransitionModels.Length; ++i)
                stateTransitionModels[i]?.DestroyImmediate();
        }

        internal static void DestroyImmediate(this StateTransitionModel stateTransitionModel)
        {
            if (stateTransitionModel == null)
                return;

            Object.DestroyImmediate(stateTransitionModel, true);
            AssetDatabase.SaveAssets();
        }

        internal static void ResetTransitions(this StateModel stateModel)
        {
            var stateTransitions = stateModel.GetTransitionsProp();
            stateTransitions.arraySize = 0;
            stateTransitions.serializedObject.ApplyModifiedProperties();
            stateModel.serializedObject().ApplyModifiedProperties();
        }

        internal static StateTransitionModel Clone(this StateTransitionModel stateTransitionModel, RuntimeStateMachineController runtimeStateMachineController, StateMachineModel toStateMachineModel)
        {
            if (!runtimeStateMachineController.IsAssetBacked() || stateTransitionModel == null || toStateMachineModel == null)
                return null;

            var clonedStateTransitionModel = Object.Instantiate(stateTransitionModel);
            clonedStateTransitionModel.name = stateTransitionModel.name;
            runtimeStateMachineController.AddObjectToAsset(clonedStateTransitionModel);
            var sourceNodeModel = stateTransitionModel.SourceNodeModel;
            var targetNodeModel = stateTransitionModel.TargetNodeModel;
            var sourceStateModel = MapClonedNode(sourceNodeModel, toStateMachineModel) as StateModel;
            var targetClonedNode = MapClonedNode(targetNodeModel, toStateMachineModel);

            if (sourceStateModel == null || targetClonedNode == null)
            {
                clonedStateTransitionModel.DestroyImmediate();
                return null;
            }

            var transitionStateModelSO = clonedStateTransitionModel.serializedObject();
            transitionStateModelSO.FindProperty(SourceNodeVar).objectReferenceValue = sourceStateModel;
            transitionStateModelSO.FindProperty(TargetNodeVar).objectReferenceValue = targetClonedNode;
            transitionStateModelSO.FindProperty(SourceStateVar).objectReferenceValue = sourceStateModel;
            transitionStateModelSO.FindProperty(TargetStateVar).objectReferenceValue = targetClonedNode as StateModel;
            transitionStateModelSO.ApplyModifiedProperties();

            var stateTranstionsList = sourceStateModel.GetTransitionsProp();
            stateTranstionsList.InsertArrayElementAtIndex(stateTranstionsList.arraySize);
            var element = stateTranstionsList.GetArrayElementAtIndex(stateTranstionsList.arraySize - 1);
            element.objectReferenceValue = clonedStateTransitionModel;

            stateTranstionsList.serializedObject.ApplyModifiedProperties();
            sourceStateModel.serializedObject().ApplyModifiedProperties();

            return clonedStateTransitionModel;
        }

        private static TransitionNodeModel MapClonedNode(TransitionNodeModel nodeModel, StateMachineModel toStateMachineModel)
        {
            if (nodeModel is StateModel stateModel)
                return toStateMachineModel.GetStateModel(stateModel.name);
            if (nodeModel is EntryStateModel)
                return toStateMachineModel.GetEntryNode();
            if (nodeModel is ExitStateModel)
                return toStateMachineModel.GetExitNode();
            if (nodeModel is StateMachineModel stateMachineModel)
                return toStateMachineModel.GetChildStateMachines().Find(child => child.name == stateMachineModel.name);

            return null;
        }

        public static string[] GetUniqueActions(this StateModel stateModel)
        {
            var actions = stateModel.serializedObject().FindProperty("m_StateActions");
            var uniqueActions = new HashSet<string>();
            for (int i = 0; i < actions.arraySize; ++i)
            {
                SerializedProperty serializedProperty = actions.GetArrayElementAtIndex(i);
                var actionName = serializedProperty.FindPropertyRelative("fullName").stringValue;
                if (!string.IsNullOrEmpty(actionName))
                    uniqueActions.Add(actionName);
            }

            return uniqueActions.ToArray();
        }
    }
}
