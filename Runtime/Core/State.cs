using System;
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
        internal IAwaitableStateAction[] _awaitableStateAction = default;
        internal TransitionState[] _transitionStates;

        private State _nextState;

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

        internal void AwaitableStateAction()
        {
            for (int i = 0; i < _awaitableStateAction.Length; ++i)
                _awaitableStateAction[i]?.Execute(_stateMachine.Actor);
        }

        internal void TryTransition(Action<string> stateChanged)
        {
            if (_nextState == null)
            {
                for (int i = 0; i < _transitionStates.Length; ++i)
                {
                    if (_transitionStates[i].TryGetTransiton(_stateMachine, out _nextState))
                    {
                        ResetExitTime();
                        return;
                    }
                }
                return;
            }

            if (IsAllAwaitableActionCompleted())
            {
                _stateMachine.CurrentState = _nextState;
                stateChanged?.Invoke(_nextState.Name);
                _nextState = null;
            }
        }

        internal void ResetExitTime()
        {
            for (int i = 0; i < _transitionStates.Length; ++i)
                _transitionStates[i].TimeElapsed = 0;
        }


        private bool IsAllAwaitableActionCompleted()
        {
            for (int i = 0; i < _awaitableStateAction.Length; ++i)
            {
                if (!_awaitableStateAction[i].IsCompleted)
                    return false;
            }
            return true;
        }

    }
}
