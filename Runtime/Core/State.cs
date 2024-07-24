using System;
using System.Collections.Generic;

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
        internal HashSet<StateEvent> _stateEnterEventForCustomTriggers = new HashSet<StateEvent>();
        internal HashSet<StateEvent> _stateExitEventForCustomTriggers = new HashSet<StateEvent>();

        private State _nextState;
        private TransitionState _transitionState;

        /// <summary>
        /// Trigger as soon as State Enter get called, before any action get executed
        /// </summary>
        public Action OnEnterEvent;

        /// <summary>
        /// Trigger as soon as State Exit get called, before any action get executed
        /// </summary>
        public Action OnExitEvent;

        private bool exitActionsExecutionStarted;

        internal State(StateMachine stateMachine, string name, string tag)
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
            foreach (var stateEnterForCustomTrigger in _stateEnterEventForCustomTriggers)
                stateEnterForCustomTrigger.Invoke();
            for (int i = 0; i < _onEnter.Length; ++i)
                _onEnter[i].Execute(ActionExecuteEvent.OnStateEnter);
        }

        internal bool OnExit()
        {
            FilterAwaitableAction(_onExit);
            var result = _awaitableStateAction.Count == 0;
            if (_onExit == null)
                return true;
            for (int i = 0; i < _onExit.Length; ++i)
                _onExit[i].Execute(ActionExecuteEvent.OnStateExit);

            foreach (var stateExitForCustomTrigger in _stateExitEventForCustomTriggers)
                stateExitForCustomTrigger.Invoke();

            OnExitEvent?.Invoke();
            return result;
        }

        internal void OnFixedUpdate()
        {
            if (_onFixedUpdate == null || exitActionsExecutionStarted)
                return;
            for (int i = 0; i < _onFixedUpdate.Length; ++i)
                _onFixedUpdate[i].Execute(ActionExecuteEvent.OnFixedUpdate);
        }

        internal void OnUpdate()
        {
            if (_onUpdate == null || exitActionsExecutionStarted)
                return;
            for (int i = 0; i < _onUpdate.Length; ++i)
                _onUpdate[i].Execute(ActionExecuteEvent.OnUpdate);
        }

        internal void OnLateUpdate()
        {
            if (_onLateUpdate == null || exitActionsExecutionStarted)
                return;
            for (int i = 0; i < _onLateUpdate.Length; ++i)
                _onLateUpdate[i].Execute(ActionExecuteEvent.OnLateUpdate);
        }

        internal void TryTransition()
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
                    bool immediateExit = OnExit();
                    exitActionsExecutionStarted = true;
                    if (immediateExit)
                    {
                        _stateMachine.nextState = _nextState;
                        _nextState = null;
                        _transitionState = null;
                    }
                }
                else
                {
                    _stateMachine.nextState = _nextState;
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

        internal void ResetTrigger()
        {
            for (int i = 0; i < _transitionStates.Length; ++i)
                _transitionStates[i].ResetTriggers(_stateMachine);
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