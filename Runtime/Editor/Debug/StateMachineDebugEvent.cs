#if UNITY_EDITOR
namespace SAS.StateMachineGraph
{
    public readonly struct StateMachineDebugEvent
    {
        internal StateMachineDebugEvent(StateMachineDebugEventType eventType, Actor actor, RuntimeStateGraph graph, ITransitionNode node,
            State state, IStateAction stateAction, TransitionState transitionState, ITransitionNode targetNode, ActionExecuteEvent actionExecuteEvent,
            int frameCount, float realtime, double timestamp, bool hasTransitionResult, bool transitionResult,
            StateMachineDebugConditionResult[] conditionResults)
        {
            EventType = eventType;
            Actor = actor;
            Graph = graph;
            Node = node;
            State = state;
            StateAction = stateAction;
            TransitionState = transitionState;
            TargetNode = targetNode;
            ActionExecuteEvent = actionExecuteEvent;
            FrameCount = frameCount;
            Realtime = realtime;
            Timestamp = timestamp;
            HasTransitionResult = hasTransitionResult;
            TransitionResult = transitionResult;
            ConditionResults = conditionResults;
        }

        public StateMachineDebugEventType EventType { get; }
        public Actor Actor { get; }
        public RuntimeStateGraph Graph { get; }
        public ITransitionNode Node { get; }
        public State State { get; }
        public IStateAction StateAction { get; }
        public TransitionState TransitionState { get; }
        public ITransitionNode TargetNode { get; }
        public ActionExecuteEvent ActionExecuteEvent { get; }
        public int FrameCount { get; }
        public float Realtime { get; }
        public double Timestamp { get; }
        public bool HasTransitionResult { get; }
        public bool TransitionResult { get; }
        public StateMachineDebugConditionResult[] ConditionResults { get; }
    }
}
#endif
