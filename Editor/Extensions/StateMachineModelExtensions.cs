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

        public static StateMachineModel AddChildStateMachine(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, string name, Vector3 position)
        {
            StateMachineModel childStateMachine = ScriptableObject.CreateInstance<StateMachineModel>();
            childStateMachine.name = stateMachineModel.MakeUniqueStateMachineName(name);

            // subStateMachine.hideFlags = HideFlags.HideInHierarchy;
            childStateMachine.SetPosition(position);
            if (AssetDatabase.GetAssetPath(runtimeStateMachineController) != "")
                AssetDatabase.AddObjectToAsset(childStateMachine, AssetDatabase.GetAssetPath(runtimeStateMachineController));
            AssetDatabase.SaveAssets();

            childStateMachine.SetAnyStatePosition(new Vector3(300, 50, 0));
            stateMachineModel.AddChildStateMachine(childStateMachine);
            return childStateMachine;
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
                    if (childStateMachineModels.GetArrayElementAtIndex(i) != null)
                        childStateMachineModels.DeleteArrayElementAtIndex(i);
                    break;
                }
            }

            parentStateMachineModel.ApplyModifiedProperties();
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
                    if (stateModelsProp.GetArrayElementAtIndex(i) != null)
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


        internal static void SetAnyStatePosition(this StateMachineModel stateMachineModel, Vector3 position)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            stateMachineModelSO.FindProperty(AnyStatePositionVar).vector3Value = position;
            stateMachineModelSO.ApplyModifiedProperties();
        }

        internal static Vector3 GetAnyStatePosition(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            return stateMachineModelSO.FindProperty(AnyStatePositionVar).vector3Value;
        }

        public static void SetPosition(this StateMachineModel stateMachineModel, Vector3 position)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            stateMachineModelSO.FindProperty(PositionVar).vector3Value = position;
            stateMachineModelSO.ApplyModifiedProperties();
        }

        public static Vector3 GetPosition(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            return stateMachineModelSO.FindProperty(PositionVar).vector3Value;
        }

        public static Vector3 GetPositionAsUpNode(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            return stateMachineModelSO.FindProperty(PositionAsUpNodeVar).vector3Value;
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
                usedNames.Add(childStateMachinesProp.GetArrayElementAtIndex(i).objectReferenceValue.name);

            return usedNames;
        }

        public static List<StateMachineModel> GetChildStateMachines(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var childStateMachinesProp = stateMachineModelSO.FindProperty(ChildStateMachinesVar);

            var childStateMachines = new List<StateMachineModel>();
            for (int i = 0; i < childStateMachinesProp.arraySize; ++i)
                childStateMachines.Add(childStateMachinesProp.GetArrayElementAtIndex(i).objectReferenceValue as StateMachineModel);

            return childStateMachines;
        }

        internal static List<StateMachineModel> GetStateMachineRecursivily(this StateMachineModel stateMachineModel)
        {
            var childStateMachines = stateMachineModel.GetChildStateMachines();
            List<StateMachineModel> stateMachineModels = new List<StateMachineModel>();
            stateMachineModels.AddRange(childStateMachines);

            for (int i = 0; i < childStateMachines.Count; i++)
                stateMachineModels.AddRange(childStateMachines[i].GetStateMachineRecursivily());

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
                stateModels.AddRange(csmm.GetStates());

            return stateModels;
        }

        internal static List<StateModel> GetStates(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var stateModelsProp = stateMachineModelSO.FindProperty(StateModelsVar);

            var stateModels = new List<StateModel>();
            for (int i = 0; i < stateModelsProp.arraySize; ++i)
                stateModels.Add(stateModelsProp.GetArrayElementAtIndex(i).objectReferenceValue as StateModel);

            return stateModels;
        }

        internal static StateMachineModel CloneMachineRecursivily(this StateMachineModel stateMachineModel, RuntimeStateMachineController runtimeStateMachineController, StateMachineModel parentStateModel, Vector3 position)
        {
            var childStateMachines = stateMachineModel.GetChildStateMachines();
            var clonedStateMachine = runtimeStateMachineController.AddChildStateMachine(parentStateModel, stateMachineModel.name, position);
            stateMachineModel.CopyStateModels(runtimeStateMachineController, clonedStateMachine);

            for (int i = 0; i < childStateMachines.Count; i++)
                childStateMachines[i].CloneMachineRecursivily(runtimeStateMachineController, clonedStateMachine, childStateMachines[i].GetPosition());

            return clonedStateMachine;
        }

        internal static StateMachineModel CloneMachineRecursivily(this StateMachineModel stateMachineModel, RuntimeStateMachineController runtimeStateMachineController, StateMachineModel parentStateModel)
        {
            var position = stateMachineModel.GetPosition() + new Vector3(35, 65);
            return stateMachineModel.CloneMachineRecursivily(runtimeStateMachineController, parentStateModel, position);
        }

        private static void CopyStateModels(this StateMachineModel fromStateMachineModel, RuntimeStateMachineController runtimeStateMachineController, StateMachineModel toStateMachineModel)
        {
            var stateModels = fromStateMachineModel.GetStates();
            foreach (var stateModel in stateModels)
                stateModel.Clone(runtimeStateMachineController, toStateMachineModel);
            
            for (int i = 0; i < stateModels.Count; i++)
            {
                var stateTransitions = stateModels[i].GetTransitionsProp();
                for (int j = 0; j < stateTransitions.arraySize; ++j)
                {
                    var stateTransitionModel = (StateTransitionModel)stateTransitions.GetArrayElementAtIndex(j).objectReferenceValue;
                    stateTransitionModel.Clone(runtimeStateMachineController, toStateMachineModel);
                }
            }
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
    }
}
