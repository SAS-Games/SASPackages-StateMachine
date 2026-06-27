namespace SAS.StateMachineGraph
{
    internal sealed class ExitNode : ITransitionNode
    {
        public ExitNode(RuntimeStateGraph graph, string name)
        {
            Graph = graph;
            Name = name;
        }

        public string Name { get; }
        public RuntimeStateGraph Graph { get; }
        public State ActiveState => null;
        public TransitionState[] TransitionStates { get; set; }

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
        }

        public void ResetTrigger()
        {
        }
    }
}
