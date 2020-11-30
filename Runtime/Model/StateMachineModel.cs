using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [CreateAssetMenu(fileName = "New State Machine Controller", menuName = "SAS/State Machine Controller")]
    public class StateMachineModel : ScriptableObject
    {
        [SerializeField] private List<StateModel> _stateModels = new List<StateModel>();
        [SerializeField] private StateMachineParameter[] _parameters;
        [SerializeField] private StateModel _defaultStateModel;
       // [SerializeField] private StateModel _anyStateModel;

        internal StateMachine CreateStateMachine(Actor actor)
        {
            StateMachine stateMachine = new StateMachine(actor, _parameters);
            var cachedState = new Dictionary<ScriptableObject, object>();
            var cachedActions = new Dictionary<StateActionModel, object>();
            foreach (StateModel stateModel in _stateModels)
            {
                var state = stateModel.GetState(stateMachine, cachedState, cachedActions);
                if (stateModel == _defaultStateModel)
                    stateMachine.DefaultState = state;
            }

            stateMachine.CurrentState = stateMachine.DefaultState;
           // stateMachine.AnyState = _anyStateModel.GetState(stateMachine, cachedState, cachedActions);

            return stateMachine;
        }

        internal void Initialize(StateMachineModel model)
        {
            name = model.name;
            _stateModels = model._stateModels;
            _parameters = new StateMachineParameter[model._parameters.Length];
            
            for(int i =0; i < _parameters.Length; ++i)
                _parameters[i] = new StateMachineParameter(model._parameters[i]);

            _defaultStateModel = model._defaultStateModel;
        }
    }
}
