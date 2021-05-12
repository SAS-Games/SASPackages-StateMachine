using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public class RuntimeStateMachineController : ScriptableObject
    {
        private const string AnyStateModelName = "Any State";
        [SerializeField] private StateMachineModel m_BaseStateMachineModel;
        [SerializeField] private List<StateModel> _stateModels = new List<StateModel>();
        [SerializeField] private StateMachineParameter[] _parameters;
        [SerializeField] private StateModel m_DefaultStateModel;
        [SerializeField] private StateModel m_AnyStateModel;

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
            foreach (StateModel stateModel in _stateModels)
            {
                var state = stateModel.GetState(stateMachine, cachedState, cachedActions);
                if (stateModel == m_DefaultStateModel)
                    stateMachine.DefaultState = state;
                else if (stateModel.name.Equals(AnyStateModelName))
                    stateMachine.AnyState = state;
            }

            stateMachine.CurrentState = stateMachine.DefaultState;
            return stateMachine;
        }

        internal void Initialize(RuntimeStateMachineController model)
        {
            name = model.name;
            _stateModels = model._stateModels;
            _parameters = new StateMachineParameter[model._parameters.Length];
            
            for(int i =0; i < _parameters.Length; ++i)
                _parameters[i] = new StateMachineParameter(model._parameters[i]);

            m_DefaultStateModel = model.m_DefaultStateModel;
        }
    }
}
