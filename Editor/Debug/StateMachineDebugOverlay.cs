using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    internal static class StateMachineDebugOverlay
    {
        private const float BreakpointRadius = 4f;
        private const int TransitionHighlightFrameWindow = 120;
        private const float TransitionHighlightWidth = 8f;
        private const float TransitionArrowSize = 18f;

        internal static void DrawNodeOverlay(BaseNode node, Actor actor, string graphName)
        {
            var session = StateMachineDebugSession.Instance;
            if (!session.IsEnabled || node == null)
                return;

            if (node is ParentStateMachineNode)
                return;

            var actorName = StateMachineDebugNodeKey.GetActorName(actor);
            if (string.IsNullOrEmpty(actorName))
                actorName = session.CurrentActorName;

            var nodeKey = StateMachineDebugNodeKey.Create(actorName, graphName, node.TargetObject);
            if (string.IsNullOrEmpty(nodeKey))
                return;

            var hasEnterBreakpoint = session.HasBreakpoint(nodeKey, StateMachineDebugEventType.NodeEnter);
            var hasExitBreakpoint = session.HasBreakpoint(nodeKey, StateMachineDebugEventType.NodeExit);
            var isBreakpoint = hasEnterBreakpoint || hasExitBreakpoint;
            var isActive = Application.isPlaying && nodeKey == session.CurrentNodeKey;
            var isPaused = Application.isPlaying && nodeKey == session.PausedNodeKey;

            if (!isBreakpoint && !isActive && !isPaused)
                return;

            var rect = node.rect.ToRect();
            if (isActive)
                DrawBorder(rect, StateMachineDebugStyles.ActiveBorder, 2f);

            if (isPaused)
            {
                DrawBorder(rect, StateMachineDebugStyles.PausedBorder, 3f);
                DrawPauseBadge(rect);
            }

            if (hasEnterBreakpoint)
                DrawBreakpointDot(rect, 10f, StateMachineDebugStyles.Breakpoint);

            if (hasExitBreakpoint)
                DrawBreakpointDot(rect, hasEnterBreakpoint ? 20f : 10f, StateMachineDebugStyles.ExitBreakpoint);
        }

        internal static void DrawConnectionOverlay(
            Connection connection,
            Actor actor,
            string graphName,
            Vector2 start,
            Vector2 end,
            bool inverted)
        {
            var session = StateMachineDebugSession.Instance;
            if (!session.IsEnabled || !Application.isPlaying || connection == null)
                return;

            if (connection.StartNode == null || connection.EndNode == null)
                return;

            if (string.IsNullOrEmpty(session.LastTransitionSourceKey) ||
                string.IsNullOrEmpty(session.LastTransitionTargetKey))
                return;

            var actorName = StateMachineDebugNodeKey.GetActorName(actor);
            if (string.IsNullOrEmpty(actorName))
                actorName = session.CurrentActorName;

            var sourceNodeName = connection.SourceNodeModel != null
                ? connection.SourceNodeModel.name
                : StateMachineDebugNodeKey.GetNodeName(connection.StartNode.TargetObject);
            var targetNodeName = connection.TargetNodeModel != null
                ? connection.TargetNodeModel.name
                : StateMachineDebugNodeKey.GetNodeName(connection.EndNode.TargetObject);

            var sourceKey = StateMachineDebugNodeKey.Create(actorName, graphName, sourceNodeName);
            var targetKey = StateMachineDebugNodeKey.Create(actorName, graphName, targetNodeName);
            if (sourceKey != session.LastTransitionSourceKey ||
                targetKey != session.LastTransitionTargetKey)
                return;

            var alpha = GetTransitionHighlightAlpha(session.LastTransitionFrame);
            if (alpha <= 0f)
                return;

            DrawTransitionHighlight(start, end, inverted, alpha);
        }

        private static void DrawBorder(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static void DrawBreakpointDot(Rect rect, float yOffset, Color color)
        {
            var previousColor = Handles.color;
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawSolidDisc(new Vector3(rect.xMax - 10f, rect.y + yOffset, 0f), Vector3.forward, BreakpointRadius);
            Handles.color = previousColor;
            Handles.EndGUI();
        }

        private static void DrawPauseBadge(Rect rect)
        {
            var badge = new Rect(rect.x + 6f, rect.y + 6f, 14f, 14f);
            EditorGUI.DrawRect(badge, StateMachineDebugStyles.PausedBorder);
            EditorGUI.DrawRect(new Rect(badge.x + 4f, badge.y + 3f, 2f, 8f), StateMachineDebugStyles.PauseGlyph);
            EditorGUI.DrawRect(new Rect(badge.x + 8f, badge.y + 3f, 2f, 8f), StateMachineDebugStyles.PauseGlyph);
        }

        private static float GetTransitionHighlightAlpha(int transitionFrame)
        {
            if (transitionFrame <= 0)
                return 0f;

            var frameAge = Mathf.Max(0, Time.frameCount - transitionFrame);
            if (frameAge > TransitionHighlightFrameWindow)
                return 0f;

            var normalizedAge = frameAge / (float)TransitionHighlightFrameWindow;
            return Mathf.Lerp(0.25f, 1f, 1f - normalizedAge);
        }

        private static void DrawTransitionHighlight(Vector2 start, Vector2 end, bool inverted, float alpha)
        {
            var color = StateMachineDebugStyles.LiveTransition;
            color.a *= alpha;

            var previousColor = Handles.color;
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawAAPolyLine(TransitionHighlightWidth, ToVector3(start), ToVector3(end));
            DrawTransitionArrow(start, end, inverted, color);
            Handles.color = previousColor;
            Handles.EndGUI();
        }

        private static void DrawTransitionArrow(Vector2 start, Vector2 end, bool inverted, Color color)
        {
            var arrowStart = start;
            var arrowEnd = end;
            if (inverted)
            {
                arrowStart = end;
                arrowEnd = start;
            }

            var direction = (arrowEnd - arrowStart).normalized;
            if (direction.sqrMagnitude <= 0f)
                return;

            var perpendicular = new Vector2(-direction.y, direction.x);
            var midpoint = Vector2.Lerp(arrowStart, arrowEnd, 0.5f);
            var arrowHead = new Vector3[3];
            arrowHead[0] = ToVector3(midpoint + direction * TransitionArrowSize * 0.55f);
            arrowHead[1] = ToVector3(midpoint - direction * TransitionArrowSize * 0.35f + perpendicular * TransitionArrowSize * 0.28f);
            arrowHead[2] = ToVector3(midpoint - direction * TransitionArrowSize * 0.35f - perpendicular * TransitionArrowSize * 0.28f);

            Handles.color = color;
            Handles.DrawAAConvexPolygon(arrowHead);
        }

        private static Vector3 ToVector3(Vector2 value)
        {
            return new Vector3(value.x, value.y, 0f);
        }
    }
}
