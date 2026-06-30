namespace SAS.StateMachineGraph.Editor
{
    internal sealed class StateMachineDebugTransitionStats
    {
        private static readonly StateMachineDebugConditionResult[] EmptyConditions =
            new StateMachineDebugConditionResult[0];

        internal StateMachineDebugTransitionStats(string sourceNodeKey, string transitionKey)
        {
            SourceNodeKey = sourceNodeKey;
            TransitionKey = transitionKey;
            TargetNodeName = string.Empty;
            TargetNodeKey = string.Empty;
            ConditionResults = EmptyConditions;
        }

        internal string SourceNodeKey { get; }
        internal string TransitionKey { get; }
        internal string TargetNodeName { get; private set; }
        internal string TargetNodeKey { get; private set; }
        internal int EvaluationCount { get; private set; }
        internal int LastEvaluatedFrame { get; private set; }
        internal float LastEvaluatedTime { get; private set; }
        internal bool HasResult { get; private set; }
        internal bool LastResult { get; private set; }
        internal StateMachineDebugConditionResult[] ConditionResults { get; private set; }

        internal void RecordEvaluation(
            string targetNodeName,
            string targetNodeKey,
            int frame,
            float time,
            bool hasResult,
            bool result,
            StateMachineDebugConditionResult[] conditionResults)
        {
            TargetNodeName = string.IsNullOrEmpty(targetNodeName) ? "<Unknown>" : targetNodeName;
            TargetNodeKey = targetNodeKey ?? string.Empty;
            EvaluationCount++;
            LastEvaluatedFrame = frame;
            LastEvaluatedTime = time;
            HasResult = hasResult;
            LastResult = result;
            ConditionResults = CopyConditionResults(conditionResults);
        }

        private static StateMachineDebugConditionResult[] CopyConditionResults(StateMachineDebugConditionResult[] conditionResults)
        {
            if (conditionResults == null || conditionResults.Length == 0)
                return EmptyConditions;

            var copy = new StateMachineDebugConditionResult[conditionResults.Length];
            for (int i = 0; i < conditionResults.Length; ++i)
                copy[i] = conditionResults[i];

            return copy;
        }
    }
}
