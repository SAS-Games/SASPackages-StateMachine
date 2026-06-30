namespace SAS.StateMachineGraph
{
    internal sealed class EntryNode : ITransitionNode
    {
        private readonly StateMachine _stateMachine;

        internal EntryNode(StateMachine stateMachine, RuntimeStateGraph graph, string name)
        {
            _stateMachine = stateMachine;
            Graph = graph;
            Name = name;
        }

        public string Name { get; }
        public RuntimeStateGraph Graph { get; }
        public State ActiveState => null;
        public TransitionState[] TransitionStates { get; set; }
        internal ITransitionNode FallbackTarget { get; set; }

        public void OnEnter()
        {
        }

        public bool OnExit()
        {
            return true;
        }

        public void OnEarlyUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public void OnUpdate()
        {
        }

        public void OnLateUpdate()
        {
        }

        public void TryTransition()
        {
            if (TransitionNodeUtility.TryGetNextNode(_stateMachine, this, TransitionStates, out var nextNode, out var transitionState))
            {
                Graph.QueueTransition(nextNode, transitionState, this);
                return;
            }

            if (FallbackTarget != null && FallbackTarget != this)
                Graph.QueueTransition(FallbackTarget, null, this);
        }

        public void ResetTrigger()
        {
            TransitionNodeUtility.ResetTriggers(TransitionStates, _stateMachine);
        }
    }
}
