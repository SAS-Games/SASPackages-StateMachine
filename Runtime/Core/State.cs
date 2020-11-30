using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    internal class State
    {
        public string Name { get; private set; }

        private StateMachine _stateMachine;
        internal IStateEnterAction[] _onEnter = default;
        internal IStateExitAction[] _onExit = default;
        internal IStateUpdateAction[] _onUpdate = default;
        internal IStateFixedUpdateAction[] _onFixedUpdate = default;
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
