using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    internal static class StateMachineDebugInspector
    {
        private static readonly ActionExecuteEvent[] ActionBuckets =
        {
            ActionExecuteEvent.OnStateEnter,
            ActionExecuteEvent.OnFixedUpdate,
            ActionExecuteEvent.OnUpdate,
            ActionExecuteEvent.OnLateUpdate,
            ActionExecuteEvent.OnStateExit
        };

        private static string _selectedActionNodeKey;
        private static string _selectedActionTypeName;
        private static ActionExecuteEvent _selectedActionExecuteEvent;

        internal static void DrawForNode(TransitionNodeModel nodeModel, RuntimeStateMachineController controller)
        {
            DrawForNode(nodeModel, controller, null, null);
        }

        internal static void DrawForState(
            StateModel stateModel,
            RuntimeStateMachineController controller,
            SerializedObject stateSerializedObject,
            Func<string, string> resolveEffectiveAction)
        {
            DrawForNode(stateModel, controller, stateSerializedObject, resolveEffectiveAction);
        }

        private static void DrawForNode(
            TransitionNodeModel nodeModel,
            RuntimeStateMachineController controller,
            SerializedObject stateSerializedObject,
            Func<string, string> resolveEffectiveAction)
        {
            var session = StateMachineDebugSession.Instance;
            if (!session.IsEnabled || nodeModel == null)
                return;

            var graphName = ResolveGraphName(controller, nodeModel, session.CurrentGraphName);
            var actorName = ResolveActorName(session);
            var nodeKey = StateMachineDebugNodeKey.Create(actorName, graphName, nodeModel.name);

            EditorGUILayout.Space(8);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Play Mode Debug", EditorStyles.boldLabel);
                DrawNodeDebugInfo(session, nodeModel.name, nodeKey);

                if (stateSerializedObject != null)
                    DrawActionDebugInfo(session, nodeKey, stateSerializedObject, resolveEffectiveAction);
            }
        }

        private static void DrawNodeDebugInfo(StateMachineDebugSession session, string nodeName, string nodeKey)
        {
            session.TryGetNodeStats(nodeKey, out var stats);

            EditorGUILayout.LabelField("Node Name", nodeName);
            EditorGUILayout.LabelField("Node Key", nodeKey);
            EditorGUILayout.LabelField("Is Active", (Application.isPlaying && nodeKey == session.CurrentNodeKey).ToString());
            EditorGUILayout.LabelField("Breakpoints", FormatBreakpoints(session, nodeKey));
            EditorGUILayout.LabelField("Is Paused Here", (Application.isPlaying && nodeKey == session.PausedNodeKey).ToString());
            if (Application.isPlaying && nodeKey == session.PausedNodeKey)
                EditorGUILayout.LabelField("Paused On", StateMachineDebugBreakpoint.GetEventLabel(session.LastBreakpointHitEventType));
            EditorGUILayout.LabelField("Enter Count", stats != null ? stats.EnterCount.ToString() : "0");
            EditorGUILayout.LabelField("Last Entered Frame", stats != null && stats.EnterCount > 0 ? stats.LastEnteredFrame.ToString() : string.Empty);
            EditorGUILayout.LabelField("Last Entered Time", stats != null && stats.EnterCount > 0 ? stats.LastEnteredTime.ToString("F3") : string.Empty);

            if (Application.isPlaying && nodeKey == session.CurrentNodeKey)
            {
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Current Runtime State", session.CurrentState != null ? session.CurrentState.Name : string.Empty);
                EditorGUILayout.LabelField("Current Runtime Action", session.CurrentAction != null ? session.CurrentAction.GetType().Name : string.Empty);
                EditorGUILayout.LabelField("Current Execute Event", session.HasCurrentActionExecuteEvent ? session.CurrentActionExecuteEvent.ToString() : string.Empty);
            }
        }

        private static void DrawActionDebugInfo(
            StateMachineDebugSession session,
            string nodeKey,
            SerializedObject stateSerializedObject,
            Func<string, string> resolveEffectiveAction)
        {
            var actions = stateSerializedObject.FindProperty("m_StateActions");
            if (actions == null || actions.arraySize == 0)
                return;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Action Debug", EditorStyles.boldLabel);

            for (int i = 0; i < ActionBuckets.Length; ++i)
                DrawActionBucket(session, nodeKey, actions, ActionBuckets[i], resolveEffectiveAction);

            DrawSelectedActionInspector(session);
        }

        private static string FormatBreakpoints(StateMachineDebugSession session, string nodeKey)
        {
            var hasEnter = session.HasBreakpoint(nodeKey, StateMachineDebugEventType.NodeEnter);
            var hasExit = session.HasBreakpoint(nodeKey, StateMachineDebugEventType.NodeExit);
            if (hasEnter && hasExit)
                return "State Enter, State Exit";

            if (hasEnter)
                return "State Enter";

            if (hasExit)
                return "State Exit";

            return "None";
        }

        private static void DrawActionBucket(
            StateMachineDebugSession session,
            string nodeKey,
            SerializedProperty actions,
            ActionExecuteEvent executeEvent,
            Func<string, string> resolveEffectiveAction)
        {
            var entries = CollectActionsForBucket(actions, executeEvent, resolveEffectiveAction);
            if (entries.Count == 0)
                return;

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField(executeEvent.ToString(), EditorStyles.miniBoldLabel);

            for (int i = 0; i < entries.Count; ++i)
                DrawActionRow(session, nodeKey, entries[i], executeEvent);
        }

        private static List<ActionDebugEntry> CollectActionsForBucket(
            SerializedProperty actions,
            ActionExecuteEvent executeEvent,
            Func<string, string> resolveEffectiveAction)
        {
            var entries = new List<ActionDebugEntry>();
            var executeMask = (int)executeEvent;
            for (int i = 0; i < actions.arraySize; ++i)
            {
                var actionProperty = actions.GetArrayElementAtIndex(i);
                var whenToExecute = actionProperty.FindPropertyRelative("whenToExecute");
                if (whenToExecute == null || (whenToExecute.intValue & executeMask) == 0)
                    continue;

                var fullNameProperty = actionProperty.FindPropertyRelative("fullName");
                var configuredFullName = fullNameProperty != null ? fullNameProperty.stringValue : string.Empty;
                var effectiveFullName = resolveEffectiveAction != null ? resolveEffectiveAction(configuredFullName) : configuredFullName;
                entries.Add(new ActionDebugEntry(GetActionTypeName(effectiveFullName)));
            }

            return entries;
        }

        private static void DrawActionRow(
            StateMachineDebugSession session,
            string nodeKey,
            ActionDebugEntry entry,
            ActionExecuteEvent executeEvent)
        {
            session.TryGetActionStats(nodeKey, entry.TypeName, executeEvent, out var stats);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Select", GUILayout.Width(55)))
                {
                    _selectedActionNodeKey = nodeKey;
                    _selectedActionTypeName = entry.TypeName;
                    _selectedActionExecuteEvent = executeEvent;
                }

                EditorGUILayout.LabelField(entry.TypeName, EditorStyles.boldLabel);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(59);
                EditorGUILayout.LabelField(FormatActionStats(stats), EditorStyles.miniLabel);
            }
        }

        private static string FormatActionStats(StateMachineDebugActionStats stats)
        {
            if (stats == null)
                return "Exec: No   Last: -   Count: 0   Frame: -   Time: -";

            var frame = stats.ExecutionCount > 0 ? stats.LastExecutedFrame.ToString() : "-";
            var time = stats.ExecutionCount > 0 ? stats.LastExecutedTime.ToString("F3") : "-";
            return $"Exec: {(stats.IsExecuting ? "Yes" : "No")}   Last: {stats.LastExecuteEvent}   Count: {stats.ExecutionCount}   Frame: {frame}   Time: {time}";
        }

        private static void DrawSelectedActionInspector(StateMachineDebugSession session)
        {
            if (string.IsNullOrEmpty(_selectedActionNodeKey) || string.IsNullOrEmpty(_selectedActionTypeName))
                return;

            EditorGUILayout.Space(6);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Selected Action Instance", EditorStyles.boldLabel);

                var action = GetSelectedActionInstance(session, out var source);
                if (action == null)
                {
                    EditorGUILayout.HelpBox("No runtime action instance has been captured for this action yet.", MessageType.Info);
                    return;
                }

                EditorGUILayout.LabelField("Source", source);
                DrawActionObject(action);
            }
        }

        private static IStateAction GetSelectedActionInstance(StateMachineDebugSession session, out string source)
        {
            source = string.Empty;
            if (session.TryGetActionStats(_selectedActionNodeKey, _selectedActionTypeName, _selectedActionExecuteEvent, out var stats) &&
                stats.LastActionInstance != null)
            {
                source = "Captured Runtime Event";
                return stats.LastActionInstance;
            }

            if (TryGetCurrentRuntimeAction(session, _selectedActionNodeKey, _selectedActionTypeName, _selectedActionExecuteEvent, out var action))
            {
                source = "Current Runtime State";
                return action;
            }

            return null;
        }

        private static bool TryGetCurrentRuntimeAction(
            StateMachineDebugSession session,
            string nodeKey,
            string actionTypeName,
            ActionExecuteEvent executeEvent,
            out IStateAction action)
        {
            action = null;
            if (session == null || session.CurrentState == null)
                return false;

            if (nodeKey != session.CurrentNodeKey && nodeKey != session.PausedNodeKey)
                return false;

            if (session.CurrentAction != null &&
                session.HasCurrentActionExecuteEvent &&
                session.CurrentActionExecuteEvent == executeEvent &&
                IsActionTypeMatch(session.CurrentAction, actionTypeName))
            {
                action = session.CurrentAction;
                return true;
            }

            var actions = GetRuntimeActions(session.CurrentState, executeEvent);
            if (actions == null)
                return false;

            for (int i = 0; i < actions.Length; ++i)
            {
                var candidate = actions[i];
                if (IsActionTypeMatch(candidate, actionTypeName))
                {
                    action = candidate;
                    return true;
                }
            }

            return false;
        }

        private static bool IsActionTypeMatch(IStateAction action, string actionTypeName)
        {
            if (action == null || string.IsNullOrEmpty(actionTypeName))
                return false;

            var type = action.GetType();
            return type.Name == actionTypeName ||
                   type.FullName == actionTypeName ||
                   SerializedType.Sanitize(type.ToString()) == actionTypeName ||
                   SerializedType.Sanitize(type.AssemblyQualifiedName) == actionTypeName;
        }

        private static IStateAction[] GetRuntimeActions(State state, ActionExecuteEvent executeEvent)
        {
            var fieldName = GetRuntimeActionFieldName(executeEvent);
            if (string.IsNullOrEmpty(fieldName))
                return null;

            var field = typeof(State).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            return field != null ? field.GetValue(state) as IStateAction[] : null;
        }

        private static string GetRuntimeActionFieldName(ActionExecuteEvent executeEvent)
        {
            switch (executeEvent)
            {
                case ActionExecuteEvent.OnStateEnter:
                    return "_onEnter";
                case ActionExecuteEvent.OnFixedUpdate:
                    return "_onFixedUpdate";
                case ActionExecuteEvent.OnUpdate:
                    return "_onUpdate";
                case ActionExecuteEvent.OnLateUpdate:
                    return "_onLateUpdate";
                case ActionExecuteEvent.OnStateExit:
                    return "_onExit";
                default:
                    return string.Empty;
            }
        }

        private static void DrawActionObject(IStateAction action)
        {
            if (action is UnityEngine.Object unityObject)
            {
                DrawUnityObject(unityObject);
                return;
            }

            DrawPlainObject(action);
        }

        private static void DrawUnityObject(UnityEngine.Object unityObject)
        {
            EditorGUILayout.ObjectField("Object", unityObject, unityObject.GetType(), true);

            try
            {
                var serializedObject = new SerializedObject(unityObject);
                var property = serializedObject.GetIterator();
                using (new EditorGUI.DisabledScope(true))
                {
                    var enterChildren = true;
                    while (property.NextVisible(enterChildren))
                    {
                        enterChildren = false;
                        EditorGUILayout.PropertyField(property, true);
                    }
                }
            }
            catch (Exception exception)
            {
                EditorGUILayout.HelpBox($"Unable to inspect Unity object: {exception.Message}", MessageType.Warning);
            }
        }

        private static void DrawPlainObject(object instance)
        {
            var type = instance.GetType();
            EditorGUILayout.LabelField("Type", type.FullName);

            using (new EditorGUI.DisabledScope(true))
            {
                DrawFields(instance, type);
            }
        }

        private static void DrawFields(object instance, Type type)
        {
            for (var currentType = type; currentType != null; currentType = currentType.BaseType)
            {
                FieldInfo[] fields;
                try
                {
                    fields = currentType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                }
                catch (Exception exception)
                {
                    EditorGUILayout.HelpBox($"Unable to inspect {currentType.Name}: {exception.Message}", MessageType.Warning);
                    continue;
                }

                for (int i = 0; i < fields.Length; ++i)
                {
                    var field = fields[i];
                    if (field.IsStatic)
                        continue;

                    DrawField(instance, field);
                }
            }
        }

        private static void DrawField(object instance, FieldInfo field)
        {
            try
            {
                var value = field.GetValue(instance);
                EditorGUILayout.LabelField(field.Name, $"{field.FieldType.Name}: {FormatValue(value)}");
            }
            catch (Exception exception)
            {
                EditorGUILayout.LabelField(field.Name, $"{field.FieldType.Name}: <{exception.GetType().Name}>");
            }
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return "null";

            if (value is UnityEngine.Object unityObject)
                return unityObject != null ? $"{unityObject.name} ({unityObject.GetType().Name})" : "null";

            try
            {
                return value.ToString();
            }
            catch
            {
                return "<unavailable>";
            }
        }

        private static string ResolveActorName(StateMachineDebugSession session)
        {
            if (!string.IsNullOrEmpty(session.CurrentActorName))
                return session.CurrentActorName;

            var selectedActor = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<Actor>()
                : null;

            return StateMachineDebugNodeKey.GetActorName(selectedActor);
        }

        private static string ResolveGraphName(
            RuntimeStateMachineController controller,
            TransitionNodeModel nodeModel,
            string fallbackGraphName)
        {
            if (controller != null)
            {
                var graphName = FindGraphName(controller, nodeModel);
                if (!string.IsNullOrEmpty(graphName))
                    return graphName;
            }

            return fallbackGraphName;
        }

        private static string FindGraphName(RuntimeStateMachineController controller, TransitionNodeModel nodeModel)
        {
            var allStateMachines = controller.GetAllStateMachines();
            for (int i = 0; i < allStateMachines.Count; ++i)
            {
                var stateMachine = allStateMachines[i];
                if (nodeModel is StateModel stateModel && stateMachine.GetStates().Contains(stateModel))
                    return stateMachine.name;

                if (stateMachine.GetEntryNode() == nodeModel || stateMachine.GetExitNode() == nodeModel)
                    return stateMachine.name;

                if (nodeModel is StateMachineModel childStateMachine &&
                    stateMachine.GetChildStateMachines().Contains(childStateMachine))
                    return stateMachine.name;
            }

            if (nodeModel is StateMachineModel stateMachineModel)
            {
                var parent = stateMachineModel.GetParent();
                return parent != null ? parent.name : stateMachineModel.name;
            }

            return string.Empty;
        }

        private static string GetActionTypeName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return "<None>";

            var type = Type.GetType(fullName);
            if (type != null)
                return type.Name;

            return SerializedType.Sanitize(fullName);
        }

        private readonly struct ActionDebugEntry
        {
            internal ActionDebugEntry(string typeName)
            {
                TypeName = typeName;
            }

            internal string TypeName { get; }
        }
    }
}
