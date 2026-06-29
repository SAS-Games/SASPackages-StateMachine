namespace SAS.StateMachineGraph
{
    internal sealed class SubStateMachine : ITransitionNode
    {
        private readonly StateMachine _stateMachine;
        private ITransitionNode _nextNode;
        private ITransitionNode _entryOverride;
        private TransitionState _transitionState;

        internal SubStateMachine(StateMachine stateMachine, RuntimeStateGraph graph, RuntimeStateGraph childGraph, string name)
        {
            _stateMachine = stateMachine;
            Graph = graph;
            ChildGraph = childGraph;
            ChildGraph.ParentGraph = graph;
            ChildGraph.OwnerSubStateMachine = this;
            Name = name;
        }

        public string Name { get; }
        public RuntimeStateGraph Graph { get; }
        internal RuntimeStateGraph ChildGraph { get; }
        public State ActiveState => ChildGraph.CurrentState;
        public TransitionState[] TransitionStates { get; set; }

        internal void SetEntryOverride(ITransitionNode entryOverride)
        {
            _entryOverride = entryOverride;
        }

        public void OnEnter()
        {
            _nextNode = null;
            _transitionState = null;
            var entryOverride = _entryOverride;
            _entryOverride = null;
            ChildGraph.Enter(entryOverride);
        }

        public bool OnExit()
        {
            if (!ChildGraph.IsComplete)
                ChildGraph.CurrentNode?.OnExit();

            return true;
        }

        public void OnEarlyUpdate()
        {
            ChildGraph.OnEarlyUpdate();
        }

        public void OnFixedUpdate()
        {
            ChildGraph.OnFixedUpdate();
        }

        public void OnUpdate()
        {
            ChildGraph.OnUpdate();
        }

        public void OnLateUpdate()
        {
            ChildGraph.OnLateUpdate();
        }

        public void TryTransition()
        {
            if (!ChildGraph.IsComplete)
            {
                ChildGraph.TryTransition();
                return;
            }

            if (_nextNode == null || _nextNode == this)
                TransitionNodeUtility.TryGetNextNode(_stateMachine, TransitionStates, out _nextNode, out _transitionState);

            if (_nextNode == null)
                return;

            Graph.QueueTransition(_nextNode);
            _nextNode = null;
            _transitionState = null;
        }

        public void ResetTrigger()
        {
            if (ChildGraph.IsComplete)
                TransitionNodeUtility.ResetTriggers(TransitionStates, _stateMachine);
            else
                ChildGraph.ResetTrigger();
        }
    }
}
