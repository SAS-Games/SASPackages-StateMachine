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

        internal bool TryGetTransition(StateMachine stateMachine, out ITransitionNode node)
        {
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
                if (!Conditions[i].IsValid(stateMachine))
                    return false;
            }

            return true;
        }

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
