namespace SAS.StateMachineGraph
{
    internal static class TransitionNodeUtility
    {
        internal static bool TryGetNextNode(StateMachine stateMachine, TransitionState[] transitionStates, out ITransitionNode nextNode, out TransitionState transitionState)
        {
            return TryGetNextNode(stateMachine, null, transitionStates, out nextNode, out transitionState);
        }

        internal static bool TryGetNextNode(StateMachine stateMachine, ITransitionNode sourceNode, TransitionState[] transitionStates, out ITransitionNode nextNode, out TransitionState transitionState)
        {
            nextNode = null;
            transitionState = null;

            if (transitionStates == null)
                return false;

            for (int i = 0; i < transitionStates.Length; ++i)
            {
                var currentTransition = transitionStates[i];
                if (currentTransition == null)
                    continue;

                StateMachineDebugRuntime.NotifyBeforeTransitionEvaluation(stateMachine.Actor, sourceNode?.Graph, sourceNode, currentTransition);
                var transitionFound = currentTransition.TryGetTransition(stateMachine, out nextNode);
                StateMachineDebugRuntime.NotifyAfterTransitionEvaluation(stateMachine.Actor, sourceNode?.Graph, sourceNode, currentTransition, nextNode, transitionFound);

                if (transitionFound)
                {
                    transitionState = currentTransition;
                    ResetExitTime(transitionStates);
                    return true;
                }
            }

            return false;
        }

        internal static void ResetExitTime(TransitionState[] transitionStates)
        {
            if (transitionStates == null)
                return;

            for (int i = 0; i < transitionStates.Length; ++i)
            {
                if (transitionStates[i] != null)
                    transitionStates[i].TimeElapsed = 0;
            }
        }

        internal static void ResetTriggers(TransitionState[] transitionStates, StateMachine stateMachine)
        {
            if (transitionStates == null)
                return;

            for (int i = 0; i < transitionStates.Length; ++i)
            {
                if (transitionStates[i] != null)
                    transitionStates[i].ResetTriggers(stateMachine);
            }
        }
    }
}
