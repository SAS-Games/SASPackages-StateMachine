using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [System.Serializable]
    public class TransitionState
    {
        private ITransitionNode TargetNode { get; }
        private bool HasExitTime { get; }
        private float ExitTime { get; }
        public bool WaitForAwaitableActionsToComplete { get; }
        private Condition[] Conditions { get; }
        private TransitionEntryOverride[] EntryOverrides { get; }

        internal float TimeElapsed = 0;

#if UNITY_EDITOR || SAS_STATE_MACHINE_DEBUG
        private StateMachineDebugConditionResult[] _lastDebugConditionResults = new StateMachineDebugConditionResult[0];

        internal StateMachineDebugConditionResult[] DebugConditionResults => _lastDebugConditionResults;
#else
        internal StateMachineDebugConditionResult[] DebugConditionResults => null;
#endif

        internal ITransitionNode DebugTargetNode => TargetNode;

        internal bool TryGetTransition(StateMachine stateMachine, out ITransitionNode node)
        {
#if UNITY_EDITOR || SAS_STATE_MACHINE_DEBUG
            PrepareDebugConditionResults();
#endif
            var timeElapsed = !HasExitTime || TimeElapsed > ExitTime;
            TimeElapsed += Time.deltaTime;
            node = timeElapsed && ShouldTransition(stateMachine) ? TargetNode : null;
            if (node != null)
                ApplyEntryOverrides();
            return node != null;
        }

        private bool ShouldTransition(StateMachine stateMachine)
        {
            if (Conditions == null)
                return true;

            for (int i = 0; i < Conditions.Length; ++i)
            {
#if UNITY_EDITOR || SAS_STATE_MACHINE_DEBUG
                var conditionResult = Conditions[i].EvaluateDebug(stateMachine, i);
                RecordDebugConditionResult(i, conditionResult);
                if (!conditionResult.Result)
                    return false;
#else
                if (!Conditions[i].IsValid(stateMachine))
                    return false;
#endif
            }

            return true;
        }

#if UNITY_EDITOR || SAS_STATE_MACHINE_DEBUG
        private void PrepareDebugConditionResults()
        {
            if (Conditions == null || Conditions.Length == 0)
            {
                _lastDebugConditionResults = new StateMachineDebugConditionResult[0];
                return;
            }

            if (_lastDebugConditionResults == null || _lastDebugConditionResults.Length != Conditions.Length)
                _lastDebugConditionResults = new StateMachineDebugConditionResult[Conditions.Length];

            for (int i = 0; i < Conditions.Length; ++i)
                _lastDebugConditionResults[i] = Conditions[i].CreateDebugConditionResult(i);
        }

        private void RecordDebugConditionResult(int index, StateMachineDebugConditionResult conditionResult)
        {
            if (_lastDebugConditionResults == null ||
                index < 0 ||
                index >= _lastDebugConditionResults.Length)
            {
                return;
            }

            _lastDebugConditionResults[index] = conditionResult;
        }
#endif

        internal void ResetTriggers(StateMachine stateMachine)
        {
            if (Conditions == null)
                return;

            for (int i = 0; i < Conditions.Length; ++i)
                Conditions[i].ResetTrigger(stateMachine);
        }

        internal TransitionState(ITransitionNode node, in Condition[] conditions, bool haxExitTime, float exitTime, bool waitForAwaitableActionsToComplete, TransitionEntryOverride[] entryOverrides = null)
        {
            TargetNode = node;
            HasExitTime = haxExitTime;
            ExitTime = exitTime;
            Conditions = conditions;
            WaitForAwaitableActionsToComplete = waitForAwaitableActionsToComplete;
            EntryOverrides = entryOverrides;
        }

        private void ApplyEntryOverrides()
        {
            if (EntryOverrides == null)
                return;

            for (int i = 0; i < EntryOverrides.Length; ++i)
                EntryOverrides[i].Apply();
        }


        internal void StateEventForCustomTrigger(ref HashSet<StateEvent> stateEnterDelegates, ref HashSet<StateEvent> stateExitDelegates)
        {
            if (Conditions == null)
                return;

            int count = Conditions.Length;
            for (int i = 0; i < count; ++i)
            {
                if (Conditions[i].Custom != null)
                {
                    stateEnterDelegates.Add(Conditions[i].Custom.OnStateEnter);
                    stateExitDelegates.Add(Conditions[i].Custom.OnStateExit);
                }
            }

        }
    }
    public delegate void StateEvent();
}
