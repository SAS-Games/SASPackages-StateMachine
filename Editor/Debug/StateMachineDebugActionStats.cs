namespace SAS.StateMachineGraph.Editor
{
    internal sealed class StateMachineDebugActionStats
    {
        internal StateMachineDebugActionStats(string actionTypeName, ActionExecuteEvent executeEvent)
        {
            ActionTypeName = actionTypeName;
            LastExecuteEvent = executeEvent;
        }

        internal string ActionTypeName { get; }
        internal ActionExecuteEvent LastExecuteEvent { get; private set; }
        internal int ExecutionCount { get; private set; }
        internal int LastExecutedFrame { get; private set; }
        internal float LastExecutedTime { get; private set; }
        internal IStateAction LastActionInstance { get; private set; }
        internal bool IsExecuting { get; private set; }

        internal void RecordBeforeExecute(ActionExecuteEvent executeEvent, IStateAction action)
        {
            LastExecuteEvent = executeEvent;
            LastActionInstance = action;
            IsExecuting = true;
        }

        internal void RecordAfterExecute(ActionExecuteEvent executeEvent, IStateAction action, int frame, float time)
        {
            LastExecuteEvent = executeEvent;
            LastActionInstance = action;
            IsExecuting = false;
            ExecutionCount++;
            LastExecutedFrame = frame;
            LastExecutedTime = time;
        }

        internal static string CreateKey(string actionTypeName, ActionExecuteEvent executeEvent)
        {
            return $"{actionTypeName}|{executeEvent}";
        }
    }
}
