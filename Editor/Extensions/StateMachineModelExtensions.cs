using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public static class StateMachineModelExtensions
    {
        const string PositionVar = "m_Position";
        const string PositionAsUpNodeVar = "m_PositionAsUpNode";
        const string AnyStatePositionVar = "m_AnyStatePosition";
        const string ParentStateMachineVar = "m_ParentStateMachine";
        const string ChildStateMachinesVar = "m_ChildStateMachines";
        const string StateModelsVar = "m_StateModels";
        const string EntryNodeVar = "m_EntryNode";
        const string ExitNodeVar = "m_ExitNode";

        public static StateMachineModel AddChildStateMachine(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, string name, Vector3Int position)
        {
            if (!runtimeStateMachineController.IsAssetBacked())
                return null;

            StateMachineModel childStateMachine = ScriptableObject.CreateInstance<StateMachineModel>();
            childStateMachine.name = stateMachineModel.MakeUniqueStateMachineName(name);

            childStateMachine.SetPosition(position);
            runtimeStateMachineController.AddObjectToAsset(childStateMachine);
            childStateMachine.SetAnyStatePosition(new Vector3Int(300, 50, 0));
            runtimeStateMachineController.EnsureEntryExitNodes(childStateMachine);
            stateMachineModel.AddChildStateMachine(childStateMachine);
            return childStateMachine;
        }

        internal static void EnsureEntryExitNodes(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            if (!runtimeStateMachineController.IsAssetBacked() || stateMachineModel == null)
                return;

            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var entryNodeProp = stateMachineModelSO.FindProperty(EntryNodeVar);
            var exitNodeProp = stateMachineModelSO.FindProperty(ExitNodeVar);

            if (entryNodeProp.objectReferenceValue == null)
            {
                var entryNode = ScriptableObject.CreateInstance<EntryStateModel>();
                entryNode.name = "Entry";
                runtimeStateMachineController.AddObjectToAsset(entryNode);
                entryNode.SetPosition(new Vector3Int(80, 100, 0));
                entryNodeProp.objectReferenceValue = entryNode;
            }

            if (exitNodeProp.objectReferenceValue == null)
            {
                var exitNode = ScriptableObject.CreateInstance<ExitStateModel>();
                exitNode.name = "Exit";
                runtimeStateMachineController.AddObjectToAsset(exitNode);
                exitNode.SetPosition(new Vector3Int(600, 100, 0));
                exitNodeProp.objectReferenceValue = exitNode;
            }

            stateMachineModelSO.ApplyModifiedProperties();
        }

        internal static EntryStateModel GetEntryNode(this StateMachineModel stateMachineModel)
        {
            return new SerializedObject(stateMachineModel).FindProperty(EntryNodeVar).objectReferenceValue as EntryStateModel;
        }

        internal static ExitStateModel GetExitNode(this StateMachineModel stateMachineModel)
        {
            return new SerializedObject(stateMachineModel).FindProperty(ExitNodeVar).objectReferenceValue as ExitStateModel;
        }

        private static void AddChildStateMachine(this StateMachineModel stateMachineModel, StateMachineModel childStateMachine)
        {
            var stateMachineSO = new SerializedObject(stateMachineModel);
            var childStateMachinesProp = stateMachineSO.FindProperty(ChildStateMachinesVar);
            childStateMachinesProp.InsertArrayElementAtIndex(childStateMachinesProp.arraySize);
            var element = childStateMachinesProp.GetArrayElementAtIndex(childStateMachinesProp.arraySize - 1);
            element.objectReferenceValue = childStateMachine;
            stateMachineSO.ApplyModifiedProperties();
            childStateMachine.SetParent(stateMachineModel);
        }

        internal static void RemoveStateMachineInternal(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var parentStateMachineModel = new SerializedObject(stateMachineModelSO.FindProperty(ParentStateMachineVar).objectReferenceValue);
            var childStateMachineModels = parentStateMachineModel.FindProperty(ChildStateMachinesVar);

            for (int i = 0; i < childStateMachineModels.arraySize; ++i)
            {
                if ((childStateMachineModels.GetArrayElementAtIndex(i).objectReferenceValue as StateMachineModel) == stateMachineModel)
                {
                    childStateMachineModels.DeleteArrayElementAtIndex(i);
                    //if (childStateMachineModels.GetArrayElementAtIndex(i) != null)
                    //    childStateMachineModels.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            parentStateMachineModel.ApplyModifiedProperties();
        }

        internal static bool IsParentOf(this StateMachineModel thisStateMachineModel, StateMachineModel stateMachineModel)
        {
            if (stateMachineModel == null || thisStateMachineModel == null)
                return false;
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var parentStateMachineModel = stateMachineModelSO.FindProperty(ParentStateMachineVar).objectReferenceValue;
            return thisStateMachineModel.Equals(parentStateMachineModel);
        }


        internal static void AddState(this StateMachineModel stateMachineModel, StateModel state)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var stateModelsProp = stateMachineModelSO.FindProperty(StateModelsVar);
            stateModelsProp.InsertArrayElementAtIndex(stateModelsProp.arraySize);
            var element = stateModelsProp.GetArrayElementAtIndex(stateModelsProp.arraySize - 1);
            element.objectReferenceValue = state;
            stateMachineModelSO.ApplyModifiedProperties();
        }

        internal static void RemoveState(this StateMachineModel stateMachineModel, StateModel stateModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var stateModelsProp = stateMachineModelSO.FindProperty(StateModelsVar);

            for (int i = 0; i < stateModelsProp.arraySize; ++i)
            {
                var element = stateModelsProp.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue == null || element.objectReferenceValue == stateModel)
                {
                    stateModelsProp.DeleteArrayElementAtIndex(i);
                    stateModelsProp.serializedObject.ApplyModifiedProperties();
                    break;
                }
            }
        }

        internal static void RemoveAllState(this StateMachineModel stateMachineModel, List<StateModel> stateModels)
        {
            foreach (var stateModel in stateModels)
                stateMachineModel.RemoveState(stateModel);
        }

        public static void SetParent(this StateMachineModel stateMachineModel, StateMachineModel parentStateMachineModel)
        {
            var childStateMachineModelSO = new SerializedObject(stateMachineModel);
            childStateMachineModelSO.FindProperty(ParentStateMachineVar).objectReferenceValue = parentStateMachineModel;
            childStateMachineModelSO.ApplyModifiedProperties();
        }

        public static StateMachineModel GetParent(this StateMachineModel stateMachineModel)
        {
            var childStateMachineModelSO = new SerializedObject(stateMachineModel);
            return childStateMachineModelSO.FindProperty(ParentStateMachineVar).objectReferenceValue as StateMachineModel;
        }


        internal static void SetAnyStatePosition(this StateMachineModel stateMachineModel, Vector3Int position)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            stateMachineModelSO.FindProperty(AnyStatePositionVar).vector3IntValue = position;
            stateMachineModelSO.ApplyModifiedProperties();
        }

        internal static Vector2Int GetAnyStatePosition(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            return (Vector2Int)stateMachineModelSO.FindProperty(AnyStatePositionVar).vector3IntValue;
        }

        public static void SetPosition(this StateMachineModel stateMachineModel, Vector3Int position)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            stateMachineModelSO.FindProperty(PositionVar).vector3IntValue = position;
            stateMachineModelSO.ApplyModifiedProperties();
        }

        public static void SetPosition(this TransitionNodeModel transitionNodeModel, Vector3Int position)
        {
            var transitionNodeModelSO = new SerializedObject(transitionNodeModel);
            transitionNodeModelSO.FindProperty(PositionVar).vector3IntValue = position;
            transitionNodeModelSO.ApplyModifiedProperties();
        }

        public static Vector3Int GetPosition(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            return stateMachineModelSO.FindProperty(PositionVar).vector3IntValue;
        }

        public static Vector3Int GetPosition(this TransitionNodeModel transitionNodeModel)
        {
            var transitionNodeModelSO = new SerializedObject(transitionNodeModel);
            return transitionNodeModelSO.FindProperty(PositionVar).vector3IntValue;
        }

        public static Vector3Int GetPositionAsUpNode(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            return stateMachineModelSO.FindProperty(PositionAsUpNodeVar).vector3IntValue;
        }

        public static void Rename(this StateMachineModel stateMachineModel, string name)
        {
            var parentStateMachineModel = new SerializedObject(stateMachineModel).FindProperty(ParentStateMachineVar).objectReferenceValue as StateMachineModel;
            var uniqueName = parentStateMachineModel.UsedStateMachineName().MakeUniqueName(name);
            stateMachineModel.name = uniqueName;
        }

        public static string MakeUniqueStateName(this StateMachineModel stateMachineModel, string name)
        {
            return MakeUniqueName(stateMachineModel.UsedStateName(), name);
        }

        public static string MakeUniqueName(this List<string> usedNames, string nameBase)
        {
            string name = nameBase;
            int counter = 1;
            while (usedNames.Contains(name.Trim()))
            {
                name = nameBase + " " + counter;
                counter++;
            }
            usedNames.Add(name);
            return name;
        }

        public static string MakeUniqueStateMachineName(this StateMachineModel stateMachineModel, string name)
        {
            return stateMachineModel.UsedStateMachineName().MakeUniqueName(name);
        }

        public static List<string> UsedStateMachineName(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var childStateMachinesProp = stateMachineModelSO.FindProperty(ChildStateMachinesVar);
            var usedNames = new List<string>();
            for (int i = 0; i < childStateMachinesProp.arraySize; ++i)
            {
                var childStateMachine = childStateMachinesProp.GetArrayElementAtIndex(i).objectReferenceValue;
                if (childStateMachine != null)
                    usedNames.Add(childStateMachine.name);
            }

            return usedNames;
        }

        public static List<StateMachineModel> GetChildStateMachines(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var childStateMachinesProp = stateMachineModelSO.FindProperty(ChildStateMachinesVar);

            var childStateMachines = new List<StateMachineModel>();
            for (int i = 0; i < childStateMachinesProp.arraySize; ++i)
            {
                var childStateMachine = childStateMachinesProp.GetArrayElementAtIndex(i).objectReferenceValue as StateMachineModel;
                if (childStateMachine != null)
                    childStateMachines.Add(childStateMachine);
            }

            return childStateMachines;
        }

        internal static List<StateMachineModel> GetStateMachineRecursivily(this StateMachineModel stateMachineModel)
        {
            var childStateMachines = stateMachineModel.GetChildStateMachines();
            List<StateMachineModel> stateMachineModels = new List<StateMachineModel>();
            stateMachineModels.AddRange(childStateMachines);

            for (int i = 0; i < childStateMachines.Count; i++)
            {
                if (childStateMachines[i] != null)
                    stateMachineModels.AddRange(childStateMachines[i].GetStateMachineRecursivily());
            }

            return stateMachineModels;
        }

        internal static bool Contains(this StateMachineModel stateMachineModel, StateModel stateModel, bool recursive = true)
        {
            return stateMachineModel.GetStatesRecursivily().IndexOf(stateModel) != -1;
        }

        internal static List<StateModel> GetStatesRecursivily(this StateMachineModel stateMachineModel)
        {
            var stateModels = new List<StateModel>();
            var childStateMachinesModel = new List<StateMachineModel>() { stateMachineModel };
            childStateMachinesModel.AddRange(stateMachineModel.GetStateMachineRecursivily());
            foreach (var csmm in childStateMachinesModel)
            {
                if (csmm != null)
                    stateModels.AddRange(csmm.GetStates());
            }

            return stateModels;
        }

        internal static List<StateModel> GetStates(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var stateModelsProp = stateMachineModelSO.FindProperty(StateModelsVar);

            var stateModels = new List<StateModel>();
            for (int i = 0; i < stateModelsProp.arraySize; ++i)
            {
                var stateModel = stateModelsProp.GetArrayElementAtIndex(i).objectReferenceValue as StateModel;
                if (stateModel != null)
                    stateModels.Add(stateModel);
            }

            return stateModels;
        }

        internal static StateMachineModel CloneMachineRecursivily(this StateMachineModel stateMachineModel, RuntimeStateMachineController runtimeStateMachineController, StateMachineModel parentStateModel, Vector3Int position)
        {
            if (!runtimeStateMachineController.IsAssetBacked() || stateMachineModel == null || parentStateModel == null)
                return null;

            var childStateMachines = stateMachineModel.GetChildStateMachines();
            var clonedStateMachine = runtimeStateMachineController.AddChildStateMachine(parentStateModel, stateMachineModel.name, position);
            if (clonedStateMachine == null)
                return null;

            stateMachineModel.CopyEntryExitLayoutTo(clonedStateMachine);
            stateMachineModel.CopyStateModels(runtimeStateMachineController, clonedStateMachine);

            for (int i = 0; i < childStateMachines.Count; i++)
                childStateMachines[i].CloneMachineRecursivily(runtimeStateMachineController, clonedStateMachine, childStateMachines[i].GetPosition());

            return clonedStateMachine;
        }

        internal static StateMachineModel CloneMachineRecursivily(this StateMachineModel stateMachineModel, RuntimeStateMachineController runtimeStateMachineController, StateMachineModel parentStateModel)
        {
            var position = stateMachineModel.GetPosition() + new Vector3Int(35, 65);
            return stateMachineModel.CloneMachineRecursivily(runtimeStateMachineController, parentStateModel, position);
        }

        private static void CopyStateModels(this StateMachineModel fromStateMachineModel, RuntimeStateMachineController runtimeStateMachineController, StateMachineModel toStateMachineModel)
        {
            if (!runtimeStateMachineController.IsAssetBacked() || fromStateMachineModel == null || toStateMachineModel == null)
                return;

            var stateModels = fromStateMachineModel.GetStates();
            foreach (var stateModel in stateModels)
                stateModel.Clone(runtimeStateMachineController, toStateMachineModel);

            for (int i = 0; i < stateModels.Count; i++)
            {
                var stateTransitions = stateModels[i].GetTransitionsProp();
                for (int j = 0; j < stateTransitions.arraySize; ++j)
                {
                    var stateTransitionModel = stateTransitions.GetArrayElementAtIndex(j).objectReferenceValue as StateTransitionModel;
                    if (stateTransitionModel == null)
                        continue;

                    stateTransitionModel.Clone(runtimeStateMachineController, toStateMachineModel);
                }
            }
        }

        private static void CopyEntryExitLayoutTo(this StateMachineModel fromStateMachineModel, StateMachineModel toStateMachineModel)
        {
            if (fromStateMachineModel == null || toStateMachineModel == null)
                return;

            var anyStatePosition = fromStateMachineModel.GetAnyStatePosition();
            toStateMachineModel.SetAnyStatePosition(new Vector3Int(anyStatePosition.x, anyStatePosition.y, 0));

            var sourceEntry = fromStateMachineModel.GetEntryNode();
            var targetEntry = toStateMachineModel.GetEntryNode();
            if (sourceEntry != null && targetEntry != null)
                targetEntry.SetPosition(sourceEntry.GetPosition());

            var sourceExit = fromStateMachineModel.GetExitNode();
            var targetExit = toStateMachineModel.GetExitNode();
            if (sourceExit != null && targetExit != null)
                targetExit.SetPosition(sourceExit.GetPosition());
        }

        internal static StateModel GetStateModel(this StateMachineModel stateMachineModel, string stateName)
        {
            var stateModels = stateMachineModel.GetStates();
            for (int i = 0; i < stateModels.Count; ++i)
            {
                if (stateModels[i].name == stateName)
                    return stateModels[i];
            }

            return null;
        }

        internal static SerializedObject serializedObject(this StateMachineModel stateMachineModel)
        {
            return new SerializedObject(stateMachineModel);
        }
    }
}
