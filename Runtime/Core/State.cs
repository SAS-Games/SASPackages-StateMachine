using System;
using System.Collections.Generic;

namespace SAS.StateMachineGraph
{
    public class State : ITransitionNode
    {
        public string Name { get; private set; }
        public string Tag { get; private set; }

        private StateMachine _stateMachine;
        private RuntimeStateGraph _graph;
        internal IStateAction[] _onEnter = default;
        internal IStateAction[] _onExit = default;
        internal IStateAction[] _onFixedUpdate = default;
        internal IStateAction[] _onUpdate = default;
        internal IStateAction[] _onLateUpdate = default;
        private List<IAwaitableStateAction> _awaitableStateAction = new List<IAwaitableStateAction>();
        internal TransitionState[] _transitionStates;
        internal HashSet<StateEvent> _stateEnterEventForCustomTriggers = new HashSet<StateEvent>();
        internal HashSet<StateEvent> _stateExitEventForCustomTriggers = new HashSet<StateEvent>();

        private ITransitionNode _nextNode;
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

        internal State(StateMachine stateMachine, RuntimeStateGraph graph, string name, string tag)
        {
            _stateMachine = stateMachine;
            _graph = graph;
            Name = name;
            Tag = tag;
        }

        RuntimeStateGraph ITransitionNode.Graph => _graph;
        State ITransitionNode.ActiveState => this;
        TransitionState[] ITransitionNode.TransitionStates
        {
            get => _transitionStates;
            set => _transitionStates = value;
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
            if (_nextNode == null || _nextNode == this)
            {
                TransitionNodeUtility.TryGetNextNode(_stateMachine, _transitionStates, out _nextNode, out _transitionState);
            }

            if (_nextNode != null && IsAllAwaitableActionCompleted())
            {
                if (!exitActionsExecutionStarted)
                {
                    bool immediateExit = OnExit();
                    exitActionsExecutionStarted = true;
                    if (immediateExit)
                    {
                        _graph.QueueTransition(_nextNode);
                        _nextNode = null;
                        _transitionState = null;
                    }
                }
                else
                {
                    _graph.QueueTransition(_nextNode);
                    _nextNode = null;
                    _transitionState = null;
                }
            }
        }

        internal void ResetTrigger()
        {
            TransitionNodeUtility.ResetTriggers(_transitionStates, _stateMachine);
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
            if (stateActions == null)
                return;

            foreach (var action in stateActions)
            {
                if (action is IAwaitableStateAction)
                    _awaitableStateAction.Add(action as IAwaitableStateAction);
            }
        }

        void ITransitionNode.OnEnter() => OnEnter();
        bool ITransitionNode.OnExit() => OnExit();
        void ITransitionNode.OnEarlyUpdate() { }
        void ITransitionNode.OnFixedUpdate() => OnFixedUpdate();
        void ITransitionNode.OnUpdate() => OnUpdate();
        void ITransitionNode.OnLateUpdate() => OnLateUpdate();
        void ITransitionNode.TryTransition() => TryTransition();
        void ITransitionNode.ResetTrigger() => ResetTrigger();
    }
}
