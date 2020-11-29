using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    internal class State
    {
        public string Name { get; private set; }

        private StateMachine _stateMachine;
        internal IStateAction[] _onEnter = default;
        internal IStateAction[] _onExit = default;
        internal IStateAction[] _onUpdate = default;
        internal IStateAction[] _onFixedUpdate = default;
        internal TransitionState[] _transitionStates;

        internal State(StateMachine stateMachine, string name)
        {
            _stateMachine = stateMachine;
            Name = name;
        }

        internal void OnEnter()
        {
            ExecuteActions(_onEnter);
        }

        internal void OnExit()
        {
            ExecuteActions(_onExit);
        }

        internal void OnUpdate()
        {
            ExecuteActions(_onUpdate);
        }

        internal void OnFixedUpdate()
        {
            ExecuteActions(_onFixedUpdate);
        }

        private void ExecuteActions(IStateAction[] actions)
        {
            for (int i = 0; i < actions.Length; ++i)
                actions[i]?.Execute(_stateMachine.Actor);
        }

        internal void TryTransition()
        {
            State state;
            for (int i = 0; i < _transitionStates.Length; ++i)
            {
                if (_transitionStates[i].TryGetTransiton(_stateMachine, out state))
                {
                    _stateMachine.CurrentState = state;
                    return;
                }
            }
        }

       /* internal void AddTransitionState(State state)
        {
            _transitionStates.Add(new TransitionState(state));
        } 

        internal void RemoveTransitionState(State state)
        {
            _transitionStates.Remove(_transitionStates.Find(ele => ele.State == state));
        }

        internal void RemoveNullTransitions()
        {
            for (int i = 0; i < _transitionStates.Count; ++i)
            {
                if (_transitionStates[i].State == null)
                    _transitionStates.RemoveAt(i);
            }
        }

        internal int GetTransitionStateIndex(State state)
        {
            return _transitionStates.IndexOf(_transitionStates.Find(ele => ele.State == state));
        }*/
    }
}
