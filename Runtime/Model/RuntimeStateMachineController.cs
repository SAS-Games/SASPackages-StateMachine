using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public class RuntimeStateMachineController : ScriptableObject
    {
        [SerializeField] private StateMachineModel m_BaseStateMachineModel = default;
        [SerializeField] private StateMachineParameter[] _parameters = default;
        [SerializeField] private StateModel m_DefaultStateModel = default;
        [SerializeField] private StateModel m_AnyStateModel = default;
        
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

        internal void Initialize(RuntimeStateMachineController model)
        {
            name = model.name;
            _parameters = new StateMachineParameter[model._parameters.Length];
            
            for(int i =0; i < _parameters.Length; ++i)
                _parameters[i] = new StateMachineParameter(model._parameters[i]);

            m_BaseStateMachineModel = model.m_BaseStateMachineModel;
            m_DefaultStateModel = model.m_DefaultStateModel;
            m_AnyStateModel = model.m_AnyStateModel;
        }
    }
}
