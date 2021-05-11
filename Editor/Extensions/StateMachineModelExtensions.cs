using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public static class StateMachineModelExtensions
    {
        const string PositionVar = "m_Position";
        const string AnyStatePositionVar = "m_AnyStatePosition";
        const string ParentStateMachineVar = "m_ParentStateMachine";
        const string ChildStateMachinesVar = "m_ChildStateMachines";
        const string StateModelsVar = "m_StateModels";

        public static void AddChildStateMachine(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, string name, Vector3 position)
        {
            StateMachineModel childStateMachine = ScriptableObject.CreateInstance<StateMachineModel>();
            childStateMachine.name = stateMachineModel.MakeUniqueStateMachineName(name);

            // subStateMachine.hideFlags = HideFlags.HideInHierarchy;
            childStateMachine.SetPosition(position);
            if (AssetDatabase.GetAssetPath(runtimeStateMachineController) != "")
                AssetDatabase.AddObjectToAsset(childStateMachine, AssetDatabase.GetAssetPath(runtimeStateMachineController));

            childStateMachine.SetAnyStatePosition(new Vector3(300, 50, 0));
            stateMachineModel.AddChildStateMachine(childStateMachine);
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
                    if (stateModelsProp.GetArrayElementAtIndex(i) == null)
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

        public static void SetParent(this StateMachineModel childStateMachineModel, StateMachineModel parentStateMachineModel)
        {
            var childStateMachineModelSO = new SerializedObject(childStateMachineModel);
            childStateMachineModelSO.FindProperty(ParentStateMachineVar).objectReferenceValue = parentStateMachineModel;
            childStateMachineModelSO.ApplyModifiedProperties();
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

        internal static List<StateModel> GetStates(this StateMachineModel stateMachineModel)
        {
            var stateMachineModelSO = new SerializedObject(stateMachineModel);
            var stateModelsProp = stateMachineModelSO.FindProperty(StateModelsVar);

            var stateModels = new List<StateModel>();
            for (int i = 0; i < stateModelsProp.arraySize; ++i)
                stateModels.Add(stateModelsProp.GetArrayElementAtIndex(i).objectReferenceValue as StateModel);

            return stateModels;
        }
    }
}
