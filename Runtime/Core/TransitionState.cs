using UnityEngine;

namespace SAS.StateMachineGraph
{
    [System.Serializable]
    internal class TransitionState
    {
        private State _targetState;
        private Condition[] _conditions;

        internal State TargetState => _targetState;

        internal bool TryGetTransiton(StateMachine stateMachine, out State state)
        {
            state = ShouldTransition(stateMachine) ? _targetState : null;
            return state != null;
        }

        private bool ShouldTransition(StateMachine stateMachine)
        {
            for (int i = 0; i < _conditions.Length; ++i)
            {
                if (!_conditions[i].IsValid(stateMachine))
                    return false;
            }

            return true;
        }

        internal TransitionState(State state, in Condition[] conditions)
        {
            _targetState = state;
            _conditions = conditions;
        }
    }
}
