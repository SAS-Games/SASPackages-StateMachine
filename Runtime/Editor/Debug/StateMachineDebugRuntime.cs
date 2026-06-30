#if UNITY_EDITOR
using System;
using System.Diagnostics;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public static class StateMachineDebugRuntime
    {
        public static event Action<StateMachineDebugEvent> EventEmitted;

        [Conditional("SAS_STATE_MACHINE_DEBUG")]
        internal static void NotifyNodeEnter(Actor actor, RuntimeStateGraph graph, ITransitionNode node)
        {
            if (node == null)
                return;
            Emit(StateMachineDebugEventType.NodeEnter, actor, graph, node, node as State, null, null, null, default, false, false, null);
        }

        [Conditional("SAS_STATE_MACHINE_DEBUG")]
        internal static void NotifyNodeExit(Actor actor, RuntimeStateGraph graph, ITransitionNode node)
        {
            if (node == null)
                return;

            Emit(StateMachineDebugEventType.NodeExit, actor, graph, node, node as State, null, null, null, default, false, false, null);
        }

        [Conditional("SAS_STATE_MACHINE_DEBUG")]
        internal static void NotifyBeforeAction(Actor actor, RuntimeStateGraph graph, State state, IStateAction stateAction, ActionExecuteEvent executeEvent)
        {
            NotifyAction(StateMachineDebugEventType.BeforeAction, actor, graph, state, stateAction, executeEvent);
        }

        [Conditional("SAS_STATE_MACHINE_DEBUG")]
        internal static void NotifyAfterAction(Actor actor, RuntimeStateGraph graph, State state, IStateAction stateAction, ActionExecuteEvent executeEvent)
        {
            NotifyAction(StateMachineDebugEventType.AfterAction, actor, graph, state, stateAction, executeEvent);
        }

        [Conditional("SAS_STATE_MACHINE_DEBUG")]
        internal static void NotifyBeforeTransitionEvaluation(Actor actor, RuntimeStateGraph graph, ITransitionNode node, TransitionState transitionState)
        {
            NotifyTransitionEvaluation(StateMachineDebugEventType.BeforeTransitionEvaluation, actor, graph, node, transitionState, null, false, false);
        }

        [Conditional("SAS_STATE_MACHINE_DEBUG")]
        internal static void NotifyAfterTransitionEvaluation(Actor actor, RuntimeStateGraph graph, ITransitionNode node, TransitionState transitionState, ITransitionNode targetNode, bool result)
        {
            NotifyTransitionEvaluation(StateMachineDebugEventType.AfterTransitionEvaluation, actor, graph, node, transitionState, targetNode, true, result);
        }

        [Conditional("SAS_STATE_MACHINE_DEBUG")]
        internal static void NotifyTransitionTaken(Actor actor, RuntimeStateGraph graph, ITransitionNode node, TransitionState transitionState, ITransitionNode targetNode)
        {
            if (targetNode == null)
                return;

            Emit(StateMachineDebugEventType.TransitionTaken, actor, graph, node, node as State, null, transitionState, targetNode, default, true, true, transitionState?.DebugConditionResults);
        }

        private static void NotifyAction(StateMachineDebugEventType eventType, Actor actor, RuntimeStateGraph graph, State state, IStateAction stateAction, ActionExecuteEvent executeEvent)
        {
            if (stateAction == null)
                return;

            Emit(eventType, actor, graph, state, state, stateAction, null, null, executeEvent, false, false, null);
        }

        private static void NotifyTransitionEvaluation(StateMachineDebugEventType eventType, Actor actor, RuntimeStateGraph graph, ITransitionNode node, TransitionState transitionState, ITransitionNode targetNode, bool hasTransitionResult, bool transitionResult)
        {
            if (transitionState == null)
                return;

            var eventTargetNode = targetNode ?? transitionState.DebugTargetNode;
            Emit(eventType, actor, graph, node, node as State, null, transitionState, eventTargetNode, default, hasTransitionResult, transitionResult, transitionState.DebugConditionResults);
        }

        private static void Emit(StateMachineDebugEventType eventType, Actor actor, RuntimeStateGraph graph, ITransitionNode node, State state, IStateAction stateAction, TransitionState transitionState, ITransitionNode targetNode, ActionExecuteEvent executeEvent, bool hasTransitionResult, bool transitionResult, StateMachineDebugConditionResult[] conditionResults)
        {
            var eventEmitted = EventEmitted;
            if (eventEmitted == null)
                return;

            eventEmitted.Invoke(new StateMachineDebugEvent(eventType, actor, graph, node, state, stateAction, transitionState, targetNode, executeEvent, Time.frameCount, Time.realtimeSinceStartup, Time.realtimeSinceStartupAsDouble, hasTransitionResult, transitionResult, conditionResults));
        }
    }
}
#endif
