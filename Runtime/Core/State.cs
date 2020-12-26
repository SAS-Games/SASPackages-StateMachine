using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    internal class State
    {
        public string Name { get; private set; }

        private StateMachine _stateMachine;
        internal IStateEnter[] _onEnter = default;
        internal IStateExit[] _onExit = default;
        internal IStateUpdate[] _onUpdate = default;
        internal IStateFixedUpdate[] _onFixedUpdate = default;
        internal TransitionState[] _transitionStates;

        internal State(StateMachine stateMachine, string name)
        {
            _stateMachine = stateMachine;
            Name = name;
        }

        internal void OnEnter()
        {
            for (int i = 0; i < _onEnter.Length; ++i)
                _onEnter[i]?.OnStateEnter(_stateMachine.Actor);
        }

        internal void OnExit()
        {
            for (int i = 0; i < _onExit.Length; ++i)
                _onExit[i]?.OnStateExit(_stateMachine.Actor);
        }

        internal void OnUpdate()
        {
            for (int i = 0; i < _onUpdate.Length; ++i)
                _onUpdate[i]?.OnUpdate(_stateMachine.Actor);
        }

        internal void OnFixedUpdate()
        {
            for (int i = 0; i < _onFixedUpdate.Length; ++i)
                _onFixedUpdate[i]?.OnFixedUpdate(_stateMachine.Actor);
        }

        internal void TryTransition()
        {
            for (int i = 0; i < _transitionStates.Length; ++i)
            {
                if (_transitionStates[i].TryGetTransiton(_stateMachine, out var state))
                {
                    state.ResetExitTime();
                    _stateMachine.CurrentState = state;
                    return;
                }
            }
        }

        internal void ResetExitTime()
        {
            for (int i = 0; i < _transitionStates.Length; ++i)
                _transitionStates[i].TimeElapsed = 0;
        }
    }
}
