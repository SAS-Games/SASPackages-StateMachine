using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public static class StateMachineControllerExtensions
    {
        const string BaseStateMachineModelVar = "m_BaseStateMachineModel";
        const string AnyStateModelVar = "m_AnyStateModel";
        const string DefaultStateModelVar = "m_DefaultStateModel";
        public static RuntimeStateMachineController CreateControllerAtPath(string path)
        {
            var controller = ScriptableObject.CreateInstance<RuntimeStateMachineController>();

            controller.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(controller, path);
            controller.AddStateMachine("Base State Machine");
            return controller;
        }

        private static void AddStateMachine(this RuntimeStateMachineController runtimeStateMachineController, string name)
        {
            StateMachineModel stateMachineModel = ScriptableObject.CreateInstance<StateMachineModel>();
            stateMachineModel.name = stateMachineModel.MakeUniqueStateMachineName(name);

            // subStateMachine.hideFlags = HideFlags.HideInHierarchy;

            if (AssetDatabase.GetAssetPath(runtimeStateMachineController) != "")
                AssetDatabase.AddObjectToAsset(stateMachineModel, AssetDatabase.GetAssetPath(runtimeStateMachineController));
            runtimeStateMachineController.AddStateMachine(stateMachineModel);
            runtimeStateMachineController.AddAnyState(stateMachineModel);
        }

        private static void AddStateMachine(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel baseStateMachineModel)
        {
            var runtimeStateMachineControllerSO = new SerializedObject(runtimeStateMachineController);
            var baseStateMachineModelProp = runtimeStateMachineControllerSO.FindProperty(BaseStateMachineModelVar);
            baseStateMachineModelProp.objectReferenceValue = baseStateMachineModel;
            runtimeStateMachineControllerSO.ApplyModifiedProperties();
        }

        private static void AddAnyState(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            var stateModel = ScriptableObject.CreateInstance<StateModel>();
            stateModel.name = runtimeStateMachineController.MakeUniqueStateName("Any State");

            // stateModel.hideFlags = HideFlags.HideInHierarchy;

            if (AssetDatabase.GetAssetPath(runtimeStateMachineController) != "")
                AssetDatabase.AddObjectToAsset(stateModel, AssetDatabase.GetAssetPath(runtimeStateMachineController));

            stateMachineModel.SetAnyStatePosition(new Vector3(300, 50, 0));
            runtimeStateMachineController.AddAnyState(stateModel);
        }

        private static void AddAnyState(this RuntimeStateMachineController runtimeStateMachineController, StateModel anyStateModel)
        {
            var runtimeStateMachineControllerSO = new SerializedObject(runtimeStateMachineController);
            var anyStateModelProp = runtimeStateMachineControllerSO.FindProperty(AnyStateModelVar);
            anyStateModelProp.objectReferenceValue = anyStateModel;
            runtimeStateMachineControllerSO.ApplyModifiedProperties();
        }


        public static string MakeUniqueStateName(this RuntimeStateMachineController runtimeStateMachineController, string name)
        {
            return MakeUniqueName(runtimeStateMachineController.UsedStateName(), name);
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

        public static List<string> UsedStateName(this RuntimeStateMachineController runtimeStateMachineController)
        {
            var usedNames = new List<string>();
            var stateMachineModels = runtimeStateMachineController.GetAllStateMachines();
            for (int i = 0; i < stateMachineModels.Count; ++i)
            {
                var stateModels = stateMachineModels[i].GetStates();
                for (int j = 0; j < stateModels.Count; ++j)
                    usedNames.Add(stateModels[j].name);
            }

            return usedNames;
        }

        internal static List<StateModel> GetAllStateModels(this RuntimeStateMachineController runtimeStateMachineController)
        {
            var allStateModels = new List<StateModel>();
            var stateMachineModels = runtimeStateMachineController.GetAllStateMachines();
            for (int i = 0; i < stateMachineModels.Count; ++i)
                allStateModels.AddRange(stateMachineModels[i].GetStates());

            return allStateModels;
        }

        internal static List<StateMachineModel> GetAllStateMachines(this RuntimeStateMachineController runtimeStateMachineController)
        {
            var runtimeStateMachineControllerSO = new SerializedObject(runtimeStateMachineController);
            var baseStateMachineModel = runtimeStateMachineControllerSO.FindProperty(BaseStateMachineModelVar).objectReferenceValue as StateMachineModel;

            List<StateMachineModel> ret = new List<StateMachineModel>() { baseStateMachineModel };
            ret.AddRange(baseStateMachineModel.GetStateMachineRecursivily());
            return ret;
        }

        internal static StateMachineModel BaseStateMachineModel(this RuntimeStateMachineController runtimeStateMachineController)
        {
            var runtimeStateMachineControllerSO = new SerializedObject(runtimeStateMachineController);
            return runtimeStateMachineControllerSO.FindProperty(BaseStateMachineModelVar).objectReferenceValue as StateMachineModel;
        }

        internal static StateMachineModel GetStateMachineModel(this RuntimeStateMachineController runtimeStateMachineController,string name)
        {
           return runtimeStateMachineController.GetAllStateMachines().Find(smm=>smm.name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

      /*  public static void AddState(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, string name)
        {
            var stateMachineSO = new SerializedObject(stateMachineModel);
            var stateModelsProp = stateMachineSO.FindProperty(StateMachineModelExtensions.StateModelsVar);

            if (stateModelsProp.arraySize > 0)
            {
                var stateModel = stateModelsProp.GetArrayElementAtIndex(stateModelsProp.arraySize - 1).objectReferenceValue as StateModel;
                runtimeStateMachineController.AddState(stateMachineModel, name, stateModel.GetPosition() + new Vector3(35, 65));
            }
            else
                runtimeStateMachineController.AddState(stateMachineModel, name, new Vector3(200, 0, 0));
        }*/

        public static void AddState(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, string name, Vector3 position)
        {
            var stateModel = ScriptableObject.CreateInstance<StateModel>();
            stateModel.name = runtimeStateMachineController.MakeUniqueStateName(name);

            // stateModel.hideFlags = HideFlags.HideInHierarchy;

            if (AssetDatabase.GetAssetPath(runtimeStateMachineController) != "")
                AssetDatabase.AddObjectToAsset(stateModel, AssetDatabase.GetAssetPath(runtimeStateMachineController));

            stateModel.SetPosition(position);
            stateMachineModel.AddState(stateModel);
        }

        internal static void RemoveState(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, StateModel stateModel)
        {
            runtimeStateMachineController.RemoveStateInternal(stateMachineModel, stateModel);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(runtimeStateMachineController));
        }

        internal static void RemoveAllState(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            var stateModels = stateMachineModel.GetStates();
            foreach (var stateModel in stateModels)
                runtimeStateMachineController.RemoveStateInternal(stateMachineModel, stateModel);
        }

        private static void RemoveStateInternal(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, StateModel stateModel)
        {
            runtimeStateMachineController.ClearAllTransition(stateModel);
            stateMachineModel.RemoveState(stateModel);
            MonoBehaviour.DestroyImmediate(stateModel, true);
        }

        internal static void RemoveStateMachine(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            var childStateMachineModels = stateMachineModel.GetChildStateMachines();
            foreach (var smm in childStateMachineModels)
                runtimeStateMachineController.RemoveStateMachineInternal(smm);

            runtimeStateMachineController.RemoveStateMachineInternal(stateMachineModel);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(runtimeStateMachineController));
        }

        private static void RemoveStateMachineInternal(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            runtimeStateMachineController.RemoveAllState(stateMachineModel);
            stateMachineModel.RemoveStateMachineInternal();
            MonoBehaviour.DestroyImmediate(stateMachineModel, true);
        }

        internal static void ClearAllTransition(this RuntimeStateMachineController runtimeStateMachineController, StateModel stateModel)
        {
            var allStateModels = runtimeStateMachineController.GetAllStateModels();
            foreach (var sm in allStateModels)
            {
                var stateTransitions = new SerializedObject(sm).FindProperty("m_Transitions");
                for (int i = 0; i < stateTransitions.arraySize; ++i)
                {
                    var element = stateTransitions.GetArrayElementAtIndex(i);

                    if (element.FindPropertyRelative("m_TargetState").objectReferenceValue == stateModel)
                        stateTransitions.DeleteArrayElementAtIndex(i);
                }

                stateTransitions.serializedObject.ApplyModifiedProperties();
            }
        }

        internal static SerializedObject AnyStateModelSO(this RuntimeStateMachineController runtimeStateMachineController)
        {
            var runtimeStateMachineControllerSO = new SerializedObject(runtimeStateMachineController);
            return new SerializedObject(runtimeStateMachineControllerSO.FindProperty(AnyStateModelVar).objectReferenceValue);
        }

        internal static StateModel GetDefaultState(this RuntimeStateMachineController runtimeStateMachineController)
        {
            var runtimeStateMachineControllerSO = new SerializedObject(runtimeStateMachineController);
            return runtimeStateMachineControllerSO.FindProperty(DefaultStateModelVar).objectReferenceValue as StateModel;
        }

        internal static void SetDefaultNode(this RuntimeStateMachineController runtimeStateMachineController, StateModel stateModel)
        {
            var runtimeStateMachineControllerSO = new SerializedObject(runtimeStateMachineController);
            runtimeStateMachineControllerSO.FindProperty(DefaultStateModelVar).objectReferenceValue = stateModel;
            runtimeStateMachineControllerSO.ApplyModifiedProperties();
        }
    }
}
