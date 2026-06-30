using System.Collections.Generic;

namespace SAS.StateMachineGraph.Editor
{
    internal sealed class StateMachineDebugNodeStats
    {
        private readonly Dictionary<string, StateMachineDebugActionStats> _actions =
            new Dictionary<string, StateMachineDebugActionStats>();

        internal StateMachineDebugNodeStats(string nodeKey)
        {
            NodeKey = nodeKey;
        }

        internal string NodeKey { get; }
        internal int EnterCount { get; private set; }
        internal int LastEnteredFrame { get; private set; }
        internal float LastEnteredTime { get; private set; }

        internal void RecordEnter(int frame, float time)
        {
            EnterCount++;
            LastEnteredFrame = frame;
            LastEnteredTime = time;
        }

        internal StateMachineDebugActionStats GetOrCreateActionStats(string actionTypeName, ActionExecuteEvent executeEvent)
        {
            var actionKey = StateMachineDebugActionStats.CreateKey(actionTypeName, executeEvent);
            if (!_actions.TryGetValue(actionKey, out var stats))
            {
                stats = new StateMachineDebugActionStats(actionTypeName, executeEvent);
                _actions.Add(actionKey, stats);
            }

            return stats;
        }

        internal bool TryGetActionStats(string actionTypeName, ActionExecuteEvent executeEvent, out StateMachineDebugActionStats stats)
        {
            return _actions.TryGetValue(StateMachineDebugActionStats.CreateKey(actionTypeName, executeEvent), out stats);
        }
    }
}
