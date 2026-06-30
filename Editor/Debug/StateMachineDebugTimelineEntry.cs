namespace SAS.StateMachineGraph.Editor
{
    internal readonly struct StateMachineDebugTimelineEntry
    {
        internal StateMachineDebugTimelineEntry(
            int frame,
            float time,
            StateMachineDebugEventType eventType,
            string actorName,
            string graphName,
            string nodeName,
            string stateName,
            string actionTypeName,
            string transitionTargetName,
            bool hasExecuteEvent,
            ActionExecuteEvent executeEvent)
        {
            Frame = frame;
            Time = time;
            EventType = eventType;
            ActorName = actorName;
            GraphName = graphName;
            NodeName = nodeName;
            StateName = stateName;
            ActionTypeName = actionTypeName;
            TransitionTargetName = transitionTargetName;
            HasExecuteEvent = hasExecuteEvent;
            ExecuteEvent = executeEvent;
        }

        internal int Frame { get; }
        internal float Time { get; }
        internal StateMachineDebugEventType EventType { get; }
        internal string ActorName { get; }
        internal string GraphName { get; }
        internal string NodeName { get; }
        internal string StateName { get; }
        internal string ActionTypeName { get; }
        internal string TransitionTargetName { get; }
        internal bool HasExecuteEvent { get; }
        internal ActionExecuteEvent ExecuteEvent { get; }
    }
}
