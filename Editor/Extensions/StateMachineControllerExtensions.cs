using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

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
            controller.AddStateMachine("Base StateMachine");
            return controller;
        }

        private static void AddStateMachine(this RuntimeStateMachineController runtimeStateMachineController, string name)
        {
            StateMachineModel stateMachineModel = ScriptableObject.CreateInstance<StateMachineModel>();
            stateMachineModel.name = stateMachineModel.MakeUniqueStateMachineName(name);

            runtimeStateMachineController.AddObjectToAsset(stateMachineModel);
            runtimeStateMachineController.AddStateMachine(stateMachineModel);
            runtimeStateMachineController.AddAnyState(stateMachineModel);
        }

        private static void AddStateMachine(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel baseStateMachineModel)
        {
            var runtimeStateMachineControllerSO = runtimeStateMachineController.ToSerializedObject();
            var baseStateMachineModelProp = runtimeStateMachineControllerSO.FindProperty(BaseStateMachineModelVar);
            baseStateMachineModelProp.objectReferenceValue = baseStateMachineModel;
            runtimeStateMachineControllerSO.ApplyModifiedProperties();
        }

        private static void AddAnyState(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            var stateModel = ScriptableObject.CreateInstance<StateModel>();
            stateModel.name = stateMachineModel.MakeUniqueStateName("Any State");

            runtimeStateMachineController.AddObjectToAsset(stateModel);
            stateMachineModel.SetAnyStatePosition(new Vector3Int(300, 50, 0));
            runtimeStateMachineController.AddAnyState(stateModel);
        }

        private static void AddAnyState(this RuntimeStateMachineController runtimeStateMachineController, StateModel anyStateModel)
        {
            var runtimeStateMachineControllerSO = runtimeStateMachineController.ToSerializedObject();
            var anyStateModelProp = runtimeStateMachineControllerSO.FindProperty(AnyStateModelVar);
            anyStateModelProp.objectReferenceValue = anyStateModel;
            runtimeStateMachineControllerSO.ApplyModifiedProperties();
        }

        internal static List<string> UsedStateName(this StateMachineModel stateMachineModel)
        {
            var usedNames = new List<string>();

            var stateModels = stateMachineModel.GetStates();
            for (int j = 0; j < stateModels.Count; ++j)
                usedNames.Add(stateModels[j].name);

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
            var baseStateMachineModel = runtimeStateMachineController.BaseStateMachineModel();

            List<StateMachineModel> ret = new List<StateMachineModel>() { baseStateMachineModel };
            ret.AddRange(baseStateMachineModel.GetStateMachineRecursivily());
            return ret;
        }

        internal static StateMachineModel BaseStateMachineModel(this RuntimeStateMachineController runtimeStateMachineController)
        {
            var runtimeStateMachineControllerSO = runtimeStateMachineController.ToSerializedObject();
            return runtimeStateMachineControllerSO.FindProperty(BaseStateMachineModelVar).objectReferenceValue as StateMachineModel;
        }

        internal static StateMachineModel GetStateMachineModel(this RuntimeStateMachineController runtimeStateMachineController, string name)
        {
            return runtimeStateMachineController.GetAllStateMachines().Find(smm => smm.name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        internal static StateMachineModel GetStateMachineModel(this RuntimeStateMachineController runtimeStateMachineController, State state)
        {
            var stateMachineModels = runtimeStateMachineController.GetAllStateMachines();
            foreach(var stateMachineModel in stateMachineModels)
            {
                if (stateMachineModel.GetStates().Find(stateModel=> stateModel.State == state))
                    return stateMachineModel;
            }

            return null;
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

        public static StateModel Clone(this StateModel stateModel, RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            var clone = Object.Instantiate(stateModel);
            clone.name = stateMachineModel.MakeUniqueStateName(stateModel.name);
            runtimeStateMachineController.CreateStateModelAsset(stateMachineModel, clone, stateModel.GetPosition() + new Vector3(35, 65));
            clone.ResetTransitions();
            return clone;
        }

        public static StateModel AddState(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, string name, Vector3 position)
        {
            var stateModel = ScriptableObject.CreateInstance<StateModel>();
            stateModel.name = stateMachineModel.MakeUniqueStateName(name);

            runtimeStateMachineController.CreateStateModelAsset(stateMachineModel, stateModel, position);

            return stateModel;
        }

        private static void CreateStateModelAsset(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, StateModel stateModel, Vector3 position)
        {
            runtimeStateMachineController.AddObjectToAsset(stateModel);
            stateModel.SetPosition(position);
            stateMachineModel.AddState(stateModel);
        }

        internal static void RemoveDefaultState(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, StateModel stateModel)
        {
            runtimeStateMachineController.SetDefaultNode(null);
            runtimeStateMachineController.RemoveStateInternal(stateMachineModel, stateModel);
        }

        internal static void RemoveState(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel, StateModel stateModel)
        {
            runtimeStateMachineController.RemoveStateInternal(stateMachineModel, stateModel);
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
            Object.DestroyImmediate(stateModel, true);
            AssetDatabase.SaveAssets();
        }

        internal static void RemoveStateMachine(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            runtimeStateMachineController.RemoveStateMachineRecursively(stateMachineModel);
            runtimeStateMachineController.RemoveStateMachineInternal(stateMachineModel);
        }

        private static void RemoveStateMachineRecursively(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            var childStateMachineModels = stateMachineModel.GetChildStateMachines();
            foreach (var smm in childStateMachineModels)
            {
                runtimeStateMachineController.RemoveStateMachineRecursively(smm);
                runtimeStateMachineController.RemoveStateMachineInternal(smm);
            }
        }

        private static void RemoveStateMachineInternal(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            runtimeStateMachineController.RemoveAllState(stateMachineModel);
            stateMachineModel.RemoveStateMachineInternal();
            Object.DestroyImmediate(stateMachineModel, true);
            AssetDatabase.SaveAssets();
        }

        internal static void ClearAllTransition(this RuntimeStateMachineController runtimeStateMachineController, StateModel targetStateModel)
        {
            var allStateModels = runtimeStateMachineController.GetAllStateModels();
            allStateModels.Remove(targetStateModel);
            allStateModels.Add(runtimeStateMachineController.AnyStateModel());
            foreach (var sourceStateModel in allStateModels)
                sourceStateModel.ClearConnection(targetStateModel);
            targetStateModel.ClearConnection();
        }

        internal static StateModel AnyStateModel(this RuntimeStateMachineController runtimeStateMachineController)
        {
            return runtimeStateMachineController.ToSerializedObject().FindProperty(AnyStateModelVar).objectReferenceValue as StateModel;
        }

        internal static StateModel GetDefaultState(this RuntimeStateMachineController runtimeStateMachineController)
        {
            return runtimeStateMachineController.ToSerializedObject().FindProperty(DefaultStateModelVar).objectReferenceValue as StateModel;
        }

        public static void Rename(this StateModel stateModel, RuntimeStateMachineController runtimeStateMachineController, string name)
        {
            var allStateMachineModel = runtimeStateMachineController.GetAllStateMachines();
            foreach (var stateMachineModel in allStateMachineModel)
            {
                var allStateModel = stateMachineModel.GetStates();
                foreach (var sm in allStateModel)
                {
                    if (sm == stateModel)
                    {
                        var uniqueName = stateMachineModel.MakeUniqueStateName(name);
                        stateModel.name = uniqueName;
                        return;
                    }
                }
            }
        }

        internal static void SetDefaultNode(this RuntimeStateMachineController runtimeStateMachineController, StateModel stateModel)
        {
            var runtimeStateMachineControllerSO = runtimeStateMachineController.ToSerializedObject();
            runtimeStateMachineControllerSO.FindProperty(DefaultStateModelVar).objectReferenceValue = stateModel;
            runtimeStateMachineControllerSO.ApplyModifiedProperties();
        }

        internal static bool IsDefaultStateMachine(this RuntimeStateMachineController runtimeStateMachineController, StateMachineModel stateMachineModel)
        {
            var defaultStateModel = runtimeStateMachineController.GetDefaultState();
            var allStateModels = stateMachineModel.GetStates();
            foreach (var stateModel in allStateModels)
            {
                if (defaultStateModel == stateModel)
                    return true;
            }

            return false;
        }
        public static SerializedObject ToSerializedObject(this RuntimeStateMachineController runtimeStateMachineController)
        {
            return new SerializedObject(runtimeStateMachineController);
        }

        internal static string[] ParametersName(this RuntimeStateMachineController runtimeStateMachineController)
        {
            var runtimeStateMachineControllerSO = runtimeStateMachineController.ToSerializedObject();
            var _parameters = runtimeStateMachineControllerSO.FindProperty("_parameters");
            var parametersName = new string[_parameters.arraySize];
            if (_parameters.arraySize > 0)
            {
                for (int i = 0; i < parametersName.Length; ++i)
                {
                    var element = _parameters.GetArrayElementAtIndex(i);
                    var name = element.FindPropertyRelative("m_Name");
                    parametersName[i] = name.stringValue;
                }
            }

            return parametersName;
        }

        internal static void AddObjectToAsset(this RuntimeStateMachineController runtimeStateMachineController, Object objectToAdd)
        {
            objectToAdd.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(objectToAdd, AssetDatabase.GetAssetPath(runtimeStateMachineController));
            AssetDatabase.SaveAssets();
        }

        internal static void HideInHierarchySubObjectsOfType<T>(this RuntimeStateMachineController runtimeStateMachineController) where T : Object
        {
            var subObjects = runtimeStateMachineController.GetSubObjectsOfType<T>();
            foreach (var subObject in subObjects)
                subObject.hideFlags = HideFlags.HideInHierarchy;
        }

        internal static List<T> GetSubObjectsOfType<T>(this RuntimeStateMachineController runtimeStateMachineController) where T : Object
        {
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(runtimeStateMachineController));
            List<T> ofType = new List<T>();
            foreach (Object o in objs)
            {
                if (o is T)
                {
                    ofType.Add(o as T);
                }
            }
            return ofType;
        }

        internal static string[] GetAllUniqueActions(this RuntimeStateMachineController runtimeStateMachineController)
        {
            var uniqueActions = new HashSet<String>();
            var allStateModels = runtimeStateMachineController.GetAllStateModels();
            for (int i = 0; i < allStateModels.Count; ++i)
            {
                var stateActions = allStateModels[i].GetUniqueActions();
                foreach (var action in stateActions)
                    uniqueActions.Add(action);
            }

            return uniqueActions.ToArray();
        }
    }
}
