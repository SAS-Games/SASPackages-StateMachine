using System;
using System.Collections.Generic;
using static SAS.StateMachineGraph.Actor;

namespace SAS.StateMachineGraph
{
    public class State
    {
        public string Name { get; private set; }
        public string Tag { get; private set; }

        private StateMachine _stateMachine;
        internal IStateAction[] _onEnter = default;
        internal IStateAction[] _onExit = default;
        internal IStateAction[] _onFixedUpdate = default;
        internal IStateAction[] _onUpdate = default;
        internal IStateAction[] _onLateUpdate = default;
        private List<IAwaitableStateAction> _awaitableStateAction = new List<IAwaitableStateAction>();
        internal TransitionState[] _transitionStates;
        internal HashSet<StateEvent> _stateEnterEventForCustomeTriggers = new HashSet<StateEvent>();
        internal HashSet<StateEvent> _stateExitEventForCustomeTriggers = new HashSet<StateEvent>();

        private State _nextState;
        private TransitionState _transitionState;
        public Action OnEnterEvent, OnExitEvent;
        private bool exitActionsExecutionStarted;

        internal State(StateMachine stateMachine, string name,string tag)
        {
            _stateMachine = stateMachine;
            Name = name;
            Tag = tag;
        }

        internal void OnEnter()
        {
            exitActionsExecutionStarted = false;
            OnEnterEvent?.Invoke();
            FilterAwaitableAction(_onEnter);
            if (_onEnter == null)
                return;
            foreach (var stateEnterForCustomTrigger in _stateEnterEventForCustomeTriggers)
                stateEnterForCustomTrigger.Invoke();
            for (int i = 0; i < _onEnter.Length; ++i)
                _onEnter[i].Execute();
        }

        internal void OnExit()
        {
            FilterAwaitableAction(_onExit);
            if (_onExit == null)
                return;
            for (int i = 0; i < _onExit.Length; ++i)
                _onExit[i].Execute();

            foreach (var stateExitForCustomTrigger in _stateExitEventForCustomeTriggers)
                stateExitForCustomTrigger.Invoke();

            OnExitEvent?.Invoke();
        }

        internal void OnFixedUpdate()
        {
            if (_onFixedUpdate == null)
                return;
            for (int i = 0; i < _onFixedUpdate.Length; ++i)
                _onFixedUpdate[i].Execute();
        }

        internal void OnUpdate()
        {
            if (_onUpdate == null)
                return;
            for (int i = 0; i < _onUpdate.Length; ++i)
                _onUpdate[i].Execute();
        }

        internal void OnLateUpdate()
        {
            if (_onLateUpdate == null)
                return;
            for (int i = 0; i < _onLateUpdate?.Length; ++i)
                _onLateUpdate[i].Execute();
        }

        internal void TryTransition(StateChanged stateChanged)
        {
            if (_nextState == null || _nextState == this)
            {
                for (int i = 0; i < _transitionStates.Length; ++i)
                {
                    if (_transitionStates[i].TryGetTransiton(_stateMachine, out _nextState))
                    {
                        _transitionState = _transitionStates[i];
                        ResetExitTime();
                        break;
                    }
                }
            }

            if (_nextState != null && IsAllAwaitableActionCompleted())
            {
                if (!exitActionsExecutionStarted)
                {
                    OnExit();
                    exitActionsExecutionStarted = true;
                }
                else
                {
                    stateChanged.Invoke(_stateMachine.CurrentState, false);
                    _stateMachine.CurrentState = _nextState;
                    stateChanged?.Invoke(_nextState, true);
                    _nextState = null;
                    _transitionState = null;
                }
            }
        }

        private void ResetExitTime()
        {
            for (int i = 0; i < _transitionStates.Length; ++i)
                _transitionStates[i].TimeElapsed = 0;
        }

        private bool IsAllAwaitableActionCompleted()
        {
            if (_transitionState != null && !_transitionState.WaitForAwaitableActionsToComplete)
                return true;
            for (int i = 0; i < _awaitableStateAction.Count; ++i)
            {
                if (!_awaitableStateAction[i].IsCompleted)
                    return false;
            }
            return true;
        }

        private void FilterAwaitableAction(IStateAction[] stateActions)
        {
            _awaitableStateAction.Clear();
            foreach (var action in stateActions)
            {
                if (action is IAwaitableStateAction)
                    _awaitableStateAction.Add(action as IAwaitableStateAction);
            }
        }
    }
}
