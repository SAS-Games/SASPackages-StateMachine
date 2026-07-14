using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace SAS.StateMachineGraph.Editor
{
    internal sealed class StateMachineDebugSession
    {
        private static readonly PropertyInfo GraphNameProperty =
            typeof(RuntimeStateGraph).GetProperty("Name", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly StateMachineDebugTransitionStats[] EmptyTransitionStats =
            new StateMachineDebugTransitionStats[0];

        internal static StateMachineDebugSession Instance { get; } = new StateMachineDebugSession();

        private readonly HashSet<StateMachineDebugBreakpoint> _breakpoints =
            new HashSet<StateMachineDebugBreakpoint>();
        private readonly Dictionary<string, StateMachineDebugNodeStats> _nodeStats =
            new Dictionary<string, StateMachineDebugNodeStats>();
        private readonly Dictionary<string, StateMachineDebugTransitionStats> _transitionStats =
            new Dictionary<string, StateMachineDebugTransitionStats>();
        private readonly Dictionary<string, List<StateMachineDebugTransitionStats>> _transitionStatsByNode =
            new Dictionary<string, List<StateMachineDebugTransitionStats>>();
        private bool _changedNotificationQueued;

        private StateMachineDebugSession()
        {
            Timeline = new StateMachineDebugTimelineBuffer();
            CurrentActorName = string.Empty;
            CurrentGraphName = string.Empty;
            CurrentNodeKey = string.Empty;
            LastTransitionSourceKey = string.Empty;
            LastTransitionTargetKey = string.Empty;
            PausedNodeKey = string.Empty;
            LastBreakpointHitKey = string.Empty;
            LastBreakpointHitEventType = StateMachineDebugEventType.NodeEnter;
        }

        internal event System.Action Changed;

        internal bool IsEnabled { get; private set; }
        internal StateMachineDebugTimelineBuffer Timeline { get; }
        internal Actor CurrentActor { get; private set; }
        internal string CurrentActorName { get; private set; }
        internal RuntimeStateGraph CurrentGraph { get; private set; }
        internal string CurrentGraphName { get; private set; }
        internal ITransitionNode CurrentNode { get; private set; }
        internal string CurrentNodeKey { get; private set; }
        internal State CurrentState { get; private set; }
        internal IStateAction CurrentAction { get; private set; }
        internal bool HasCurrentActionExecuteEvent { get; private set; }
        internal ActionExecuteEvent CurrentActionExecuteEvent { get; private set; }
        internal TransitionState LastTransition { get; private set; }
        internal ITransitionNode LastTransitionSource { get; private set; }
        internal ITransitionNode LastTransitionTarget { get; private set; }
        internal string LastTransitionSourceKey { get; private set; }
        internal string LastTransitionTargetKey { get; private set; }
        internal int LastTransitionFrame { get; private set; }
        internal float LastTransitionTime { get; private set; }
        internal ITransitionNode PausedNode { get; private set; }
        internal string PausedNodeKey { get; private set; }
        internal string LastBreakpointHitKey { get; private set; }
        internal StateMachineDebugEventType LastBreakpointHitEventType { get; private set; }
        internal int LastBreakpointHitFrame { get; private set; }
        internal IEnumerable<StateMachineDebugBreakpoint> Breakpoints => _breakpoints;
        internal int BreakpointCount => _breakpoints.Count;

        internal void Enable()
        {
            if (IsEnabled)
                return;

            StateMachineDebugRuntime.EventEmitted += OnDebugEvent;
            EditorApplication.pauseStateChanged += OnEditorPauseStateChanged;
            IsEnabled = true;
            NotifyChanged();
        }

        internal void Disable()
        {
            if (!IsEnabled)
                return;

            StateMachineDebugRuntime.EventEmitted -= OnDebugEvent;
            EditorApplication.pauseStateChanged -= OnEditorPauseStateChanged;
            IsEnabled = false;
            NotifyChanged();
        }

        internal void Pause()
        {
            if (!EditorApplication.isPlaying)
                return;

            EditorApplication.isPaused = true;
            NotifyChanged();
        }

        internal void Resume()
        {
            if (!EditorApplication.isPlaying)
                return;

            ClearPauseState();
            EditorApplication.isPaused = false;
            NotifyChanged();
        }

        internal void StepFrame()
        {
            if (!EditorApplication.isPlaying)
                return;

            ClearPauseState();
            if (!EditorApplication.isPaused)
                EditorApplication.isPaused = true;

            EditorApplication.Step();
            NotifyChanged();
        }

        internal void Clear()
        {
            Timeline.Clear();
            _nodeStats.Clear();
            _transitionStats.Clear();
            _transitionStatsByNode.Clear();
            CurrentActor = null;
            CurrentActorName = string.Empty;
            CurrentGraph = null;
            CurrentGraphName = string.Empty;
            CurrentNode = null;
            CurrentNodeKey = string.Empty;
            CurrentState = null;
            CurrentAction = null;
            HasCurrentActionExecuteEvent = false;
            CurrentActionExecuteEvent = default;
            LastTransition = null;
            LastTransitionSource = null;
            LastTransitionTarget = null;
            LastTransitionSourceKey = string.Empty;
            LastTransitionTargetKey = string.Empty;
            LastTransitionFrame = 0;
            LastTransitionTime = 0f;
            PausedNode = null;
            PausedNodeKey = string.Empty;
            LastBreakpointHitKey = string.Empty;
            LastBreakpointHitEventType = StateMachineDebugEventType.NodeEnter;
            LastBreakpointHitFrame = 0;
            NotifyChanged();
        }

        internal void AddBreakpoint(string nodeKey)
        {
            AddBreakpoint(nodeKey, StateMachineDebugEventType.NodeEnter);
        }

        internal void AddBreakpoint(string nodeKey, StateMachineDebugEventType eventType)
        {
            if (string.IsNullOrEmpty(nodeKey))
                return;

            if (!IsBreakpointEvent(eventType))
                return;

            if (_breakpoints.Add(new StateMachineDebugBreakpoint(nodeKey, eventType)))
                NotifyChanged();
        }

        internal void RemoveBreakpoint(string nodeKey)
        {
            if (string.IsNullOrEmpty(nodeKey))
                return;

            var removed = _breakpoints.Remove(new StateMachineDebugBreakpoint(nodeKey, StateMachineDebugEventType.NodeEnter));
            removed |= _breakpoints.Remove(new StateMachineDebugBreakpoint(nodeKey, StateMachineDebugEventType.NodeExit));
            if (removed)
                NotifyChanged();
        }

        internal void RemoveBreakpoint(string nodeKey, StateMachineDebugEventType eventType)
        {
            if (string.IsNullOrEmpty(nodeKey))
                return;

            if (!IsBreakpointEvent(eventType))
                return;

            if (_breakpoints.Remove(new StateMachineDebugBreakpoint(nodeKey, eventType)))
                NotifyChanged();
        }

        internal void ToggleBreakpoint(string nodeKey)
        {
            ToggleBreakpoint(nodeKey, StateMachineDebugEventType.NodeEnter);
        }

        internal void ToggleBreakpoint(string nodeKey, StateMachineDebugEventType eventType)
        {
            if (string.IsNullOrEmpty(nodeKey))
                return;

            if (!IsBreakpointEvent(eventType))
                return;

            var breakpoint = new StateMachineDebugBreakpoint(nodeKey, eventType);
            if (_breakpoints.Contains(breakpoint))
                _breakpoints.Remove(breakpoint);
            else
                _breakpoints.Add(breakpoint);

            NotifyChanged();
        }

        internal bool HasBreakpoint(string nodeKey)
        {
            return HasBreakpoint(nodeKey, StateMachineDebugEventType.NodeEnter) ||
                   HasBreakpoint(nodeKey, StateMachineDebugEventType.NodeExit);
        }

        internal bool HasBreakpoint(string nodeKey, StateMachineDebugEventType eventType)
        {
            return !string.IsNullOrEmpty(nodeKey) &&
                   IsBreakpointEvent(eventType) &&
                   _breakpoints.Contains(new StateMachineDebugBreakpoint(nodeKey, eventType));
        }

        internal void ClearBreakpoints()
        {
            if (_breakpoints.Count == 0)
                return;

            _breakpoints.Clear();
            NotifyChanged();
        }

        internal bool TryGetNodeStats(string nodeKey, out StateMachineDebugNodeStats stats)
        {
            if (string.IsNullOrEmpty(nodeKey))
            {
                stats = null;
                return false;
            }

            return _nodeStats.TryGetValue(nodeKey, out stats);
        }

        internal bool TryGetActionStats(string nodeKey, string actionTypeName, ActionExecuteEvent executeEvent, out StateMachineDebugActionStats stats)
        {
            stats = null;
            return TryGetNodeStats(nodeKey, out var nodeStats) &&
                   nodeStats.TryGetActionStats(actionTypeName, executeEvent, out stats);
        }

        internal IReadOnlyList<StateMachineDebugTransitionStats> GetTransitionStats(string nodeKey)
        {
            if (string.IsNullOrEmpty(nodeKey) ||
                !_transitionStatsByNode.TryGetValue(nodeKey, out var stats))
            {
                return EmptyTransitionStats;
            }

            return stats;
        }

        private void OnDebugEvent(StateMachineDebugEvent debugEvent)
        {
            UpdateCurrentState(debugEvent);
            UpdateStats(debugEvent);
            Timeline.Add(CreateTimelineEntry(debugEvent));
            TryHandleBreakpoint(debugEvent);
            NotifyChanged();
        }

        private void UpdateCurrentState(StateMachineDebugEvent debugEvent)
        {
            CurrentActor = debugEvent.Actor;
            CurrentActorName = StateMachineDebugNodeKey.GetActorName(debugEvent.Actor);
            CurrentGraph = debugEvent.Graph;
            CurrentGraphName = GetGraphName(debugEvent.Graph);
            CurrentNode = debugEvent.Node;
            CurrentNodeKey = StateMachineDebugNodeKey.Create(CurrentActorName, CurrentGraphName, StateMachineDebugNodeKey.GetNodeName(CurrentNode));
            CurrentState = debugEvent.State ?? debugEvent.Node?.ActiveState;

            if (IsActionEvent(debugEvent.EventType))
            {
                CurrentAction = debugEvent.StateAction;
                CurrentActionExecuteEvent = debugEvent.ActionExecuteEvent;
                HasCurrentActionExecuteEvent = true;
            }

            if (debugEvent.EventType == StateMachineDebugEventType.TransitionTaken)
            {
                LastTransition = debugEvent.TransitionState;
                LastTransitionSource = debugEvent.Node;
                LastTransitionTarget = debugEvent.TargetNode;
                LastTransitionSourceKey = CurrentNodeKey;
                LastTransitionTargetKey = StateMachineDebugNodeKey.Create(
                    CurrentActorName,
                    CurrentGraphName,
                    StateMachineDebugNodeKey.GetNodeName(debugEvent.TargetNode));
                LastTransitionFrame = debugEvent.FrameCount;
                LastTransitionTime = debugEvent.Realtime;
            }
        }

        private void UpdateStats(StateMachineDebugEvent debugEvent)
        {
            if (string.IsNullOrEmpty(CurrentNodeKey))
                return;

            var nodeStats = GetOrCreateNodeStats(CurrentNodeKey);
            if (debugEvent.EventType == StateMachineDebugEventType.NodeEnter)
            {
                nodeStats.RecordEnter(debugEvent.FrameCount, debugEvent.Realtime);
                return;
            }

            if (debugEvent.EventType == StateMachineDebugEventType.AfterTransitionEvaluation)
            {
                RecordTransitionEvaluation(debugEvent);
                return;
            }

            if (!IsActionEvent(debugEvent.EventType) || debugEvent.StateAction == null)
                return;

            var actionTypeName = GetActionTypeName(debugEvent.StateAction);
            var actionStats = nodeStats.GetOrCreateActionStats(actionTypeName, debugEvent.ActionExecuteEvent);
            if (debugEvent.EventType == StateMachineDebugEventType.BeforeAction)
                actionStats.RecordBeforeExecute(debugEvent.ActionExecuteEvent, debugEvent.StateAction);
            else
                actionStats.RecordAfterExecute(debugEvent.ActionExecuteEvent, debugEvent.StateAction, debugEvent.FrameCount, debugEvent.Realtime);
        }

        private void RecordTransitionEvaluation(StateMachineDebugEvent debugEvent)
        {
            if (debugEvent.TransitionState == null || string.IsNullOrEmpty(CurrentNodeKey))
                return;

            var transitionKey = CreateTransitionStatsKey(CurrentNodeKey, debugEvent.TransitionState);
            if (!_transitionStats.TryGetValue(transitionKey, out var stats))
            {
                stats = new StateMachineDebugTransitionStats(CurrentNodeKey, transitionKey);
                _transitionStats.Add(transitionKey, stats);

                if (!_transitionStatsByNode.TryGetValue(CurrentNodeKey, out var nodeTransitions))
                {
                    nodeTransitions = new List<StateMachineDebugTransitionStats>();
                    _transitionStatsByNode.Add(CurrentNodeKey, nodeTransitions);
                }

                nodeTransitions.Add(stats);
            }

            var targetName = StateMachineDebugNodeKey.GetNodeName(debugEvent.TargetNode);
            var targetKey = StateMachineDebugNodeKey.Create(CurrentActorName, CurrentGraphName, targetName);
            stats.RecordEvaluation(
                targetName,
                targetKey,
                debugEvent.FrameCount,
                debugEvent.Realtime,
                debugEvent.HasTransitionResult,
                debugEvent.TransitionResult,
                debugEvent.ConditionResults);
        }

        private StateMachineDebugNodeStats GetOrCreateNodeStats(string nodeKey)
        {
            if (!_nodeStats.TryGetValue(nodeKey, out var stats))
            {
                stats = new StateMachineDebugNodeStats(nodeKey);
                _nodeStats.Add(nodeKey, stats);
            }

            return stats;
        }

        private void TryHandleBreakpoint(StateMachineDebugEvent debugEvent)
        {
            if (!IsBreakpointEvent(debugEvent.EventType))
                return;

            if (!HasBreakpoint(CurrentNodeKey, debugEvent.EventType))
                return;

            PausedNode = CurrentNode;
            PausedNodeKey = CurrentNodeKey;
            LastBreakpointHitKey = CurrentNodeKey;
            LastBreakpointHitEventType = debugEvent.EventType;
            LastBreakpointHitFrame = debugEvent.FrameCount;
            EditorApplication.isPaused = true;
        }

        private void OnEditorPauseStateChanged(PauseState pauseState)
        {
            if (pauseState != PauseState.Unpaused)
                return;

            if (ClearPauseState())
                NotifyChanged();
        }

        private bool ClearPauseState()
        {
            if (PausedNode == null && string.IsNullOrEmpty(PausedNodeKey))
                return false;

            PausedNode = null;
            PausedNodeKey = string.Empty;
            return true;
        }

        private StateMachineDebugTimelineEntry CreateTimelineEntry(StateMachineDebugEvent debugEvent)
        {
            var hasExecuteEvent = IsActionEvent(debugEvent.EventType);
            return new StateMachineDebugTimelineEntry(
                debugEvent.FrameCount,
                debugEvent.Realtime,
                debugEvent.EventType,
                CurrentActorName,
                CurrentGraphName,
                StateMachineDebugNodeKey.GetNodeName(debugEvent.Node),
                GetStateName(debugEvent.State ?? debugEvent.Node?.ActiveState),
                GetActionTypeName(debugEvent.StateAction),
                StateMachineDebugNodeKey.GetNodeName(debugEvent.TargetNode),
                hasExecuteEvent,
                hasExecuteEvent ? debugEvent.ActionExecuteEvent : default);
        }

        private static bool IsActionEvent(StateMachineDebugEventType eventType)
        {
            return eventType == StateMachineDebugEventType.BeforeAction ||
                   eventType == StateMachineDebugEventType.AfterAction;
        }

        private static bool IsBreakpointEvent(StateMachineDebugEventType eventType)
        {
            return eventType == StateMachineDebugEventType.NodeEnter ||
                   eventType == StateMachineDebugEventType.NodeExit;
        }

        private static string CreateTransitionStatsKey(string nodeKey, TransitionState transitionState)
        {
            return $"{nodeKey}|{RuntimeHelpers.GetHashCode(transitionState)}";
        }

        private static string GetGraphName(RuntimeStateGraph graph)
        {
            if (graph == null)
                return string.Empty;

            var graphName = GraphNameProperty?.GetValue(graph, null) as string;
            return string.IsNullOrEmpty(graphName) ? graph.GetType().Name : graphName;
        }

        private static string GetStateName(State state)
        {
            return state != null ? state.Name : string.Empty;
        }

        private static string GetActionTypeName(IStateAction action)
        {
            return action != null ? action.GetType().Name : string.Empty;
        }

        private void NotifyChanged()
        {
            if (_changedNotificationQueued)
                return;

            _changedNotificationQueued = true;
            EditorApplication.delayCall += NotifyChangedDelayed;
        }

        private void NotifyChangedDelayed()
        {
            _changedNotificationQueued = false;
            Changed?.Invoke();
        }
    }

    internal struct StateMachineDebugBreakpoint : IEquatable<StateMachineDebugBreakpoint>
    {
        internal StateMachineDebugBreakpoint(string nodeKey, StateMachineDebugEventType eventType)
        {
            NodeKey = nodeKey;
            EventType = eventType;
        }

        internal string NodeKey { get; }
        internal StateMachineDebugEventType EventType { get; }

        public bool Equals(StateMachineDebugBreakpoint other)
        {
            return string.Equals(NodeKey, other.NodeKey, StringComparison.Ordinal) &&
                   EventType == other.EventType;
        }

        public override bool Equals(object obj)
        {
            return obj is StateMachineDebugBreakpoint breakpoint && Equals(breakpoint);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((NodeKey != null ? NodeKey.GetHashCode() : 0) * 397) ^ (int)EventType;
            }
        }

        public override string ToString()
        {
            return $"{NodeKey} ({GetEventLabel(EventType)})";
        }

        internal static string GetEventLabel(StateMachineDebugEventType eventType)
        {
            switch (eventType)
            {
                case StateMachineDebugEventType.NodeEnter:
                    return "State Enter";
                case StateMachineDebugEventType.NodeExit:
                    return "State Exit";
                default:
                    return eventType.ToString();
            }
        }
    }
}
