using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public class RuntimeStateMachineController : ScriptableObject
    {
        [SerializeField, HideInInspector] private StateMachineModel m_BaseStateMachineModel = default;
        [SerializeField, HideInInspector] private StateMachineParameter[] _parameters = default;
        [SerializeField, HideInInspector] private StateModel m_DefaultStateModel = default;
        [SerializeField, HideInInspector] private StateModel m_AnyStateModel = default;

        private void Awake()
        {
#if UNITY_EDITOR
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            name = fileName;
#endif
        }

        internal StateMachine CreateStateMachine(Actor actor)
        {
            StateMachine stateMachine = new StateMachine(actor, _parameters);
            var cachedState = new Dictionary<ScriptableObject, object>();
            var cachedActions = new Dictionary<StateActionModel, object[]>();

            var stateModels = m_BaseStateMachineModel.GetStatesRecursivily();
            stateModels.Add(m_AnyStateModel);

            foreach (StateModel stateModel in stateModels)
            {
                var state = stateModel.GetState(stateMachine, cachedState, cachedActions);
                if (stateModel == m_DefaultStateModel)
                    stateMachine.DefaultState = state;
                else if (stateModel == m_AnyStateModel)
                    stateMachine.AnyState = state;
            }

            stateMachine.CurrentState = stateMachine.DefaultState;
            return stateMachine;
        }

        internal void Initialize(RuntimeStateMachineController runtimeStateMachineController)
        {
            name = runtimeStateMachineController.name;
            _parameters = new StateMachineParameter[runtimeStateMachineController._parameters.Length];

            for (int i = 0; i < _parameters.Length; ++i)
                _parameters[i] = new StateMachineParameter(runtimeStateMachineController._parameters[i]);

            m_BaseStateMachineModel = runtimeStateMachineController.m_BaseStateMachineModel;
            m_DefaultStateModel = runtimeStateMachineController.m_DefaultStateModel;
            m_AnyStateModel = runtimeStateMachineController.m_AnyStateModel;
        }
    }
}
