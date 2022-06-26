using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [System.Serializable]
    internal class TransitionState
    {
        private State TargetState { get; }
        private bool HasExitTime { get; }
        private float ExitTime{ get; }
        public bool WaitForAwaitableActionsToComplete { get; }
        private Condition[] Conditions { get; }

        internal float TimeElapsed = 0;

        internal bool TryGetTransiton(StateMachine stateMachine, out State state)
        {
            var timeElapsed = !HasExitTime || TimeElapsed > ExitTime;
            TimeElapsed += Time.deltaTime;
            state = timeElapsed && ShouldTransition(stateMachine) ? TargetState : null;
            return state != null;
        }

        private bool ShouldTransition(StateMachine stateMachine)
        {
            for (int i = 0; i < Conditions.Length; ++i)
            {
                if (!Conditions[i].IsValid(stateMachine))
                    return false;
            }

            return true;
        }

        internal TransitionState(State state, in Condition[] conditions, bool haxExitTime, float exitTime, bool waitForAwaitableActionsToComplete)
        {
            TargetState = state;
            HasExitTime = haxExitTime;
            ExitTime = exitTime;
            Conditions = conditions;
            WaitForAwaitableActionsToComplete = waitForAwaitableActionsToComplete;
        }


        internal void StateEventForCustomTrigger(ref HashSet<StateEvent> stateEnterDelegates, ref HashSet<StateEvent> stateExitDelegates)
        {
            int count = Conditions.Length;
            for(int i =0; i < count; ++i)
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
