using System;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class Connection
    {
        private const float ReciprocalConnectionOffset = 8f;
        private const float SelfConnectionOffset = 42f;

        public BaseNode StartNode { get; }
        public BaseNode EndNode { get; }

        public TransitionNodeModel SourceNodeModel { get; }
        public TransitionNodeModel TargetNodeModel { get; }

        private Vector2 _startPos;
        private Vector2 _endPos;
        private bool _hasLine;
        private bool _isInverted;
        private Vector3[] _linePoints;

        private Action<Connection> _removeConnection;

        public Connection(BaseNode start, BaseNode end, TransitionNodeModel sourceNodeModel, TransitionNodeModel targetNodeModel, Action<Connection> removeConnection)
        {
            StartNode = start;
            EndNode = end;
            SourceNodeModel = sourceNodeModel;
            TargetNodeModel = targetNodeModel;
            _removeConnection = removeConnection;
        }

        private void DrawConnection()
        {
            _hasLine = false;
            _isInverted = false;
            _linePoints = null;
            if (StartNode == null || EndNode == null)
                return;

            if (SourceNodeModel != null && SourceNodeModel == TargetNodeModel)
            {
                DrawSelfConnection();
                return;
            }

            var sourceRect = StartNode.rect.ToRect();
            var targetRect = EndNode.rect.ToRect();
            var sourceCenter = sourceRect.center;
            var targetCenter = targetRect.center;
            var direction = targetCenter - sourceCenter;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                direction = Vector2.right;

            direction.Normalize();
            var offset = GetReciprocalOffset(direction);

            var sourceOrigin = sourceCenter + offset;
            var targetOrigin = targetCenter + offset;
            _startPos = GetRectEdgePoint(sourceRect, sourceOrigin, direction);
            _endPos = GetRectEdgePoint(targetRect, targetOrigin, -direction);
            _linePoints = new[] { ToVector3(_startPos), ToVector3(_endPos) };

            DrawLinePoints();
            DrawArrow();

            _hasLine = true;
        }

        private void DrawLinePoints()
        {
            Handles.color = Color.grey;
            Handles.DrawAAPolyLine(5f, _linePoints);
        }

        private void DrawSelfConnection()
        {
            var rect = StartNode.rect.ToRect();
            var y = rect.yMax;
            _startPos = new Vector2(rect.xMax - 28f, y);
            _endPos = new Vector2(rect.x + 28f, y);

            var right = new Vector2(rect.xMax + SelfConnectionOffset, y + SelfConnectionOffset * 0.45f);
            var bottom = new Vector2(rect.center.x, y + SelfConnectionOffset);
            var left = new Vector2(rect.x - SelfConnectionOffset, y + SelfConnectionOffset * 0.45f);

            _linePoints = new[]
            {
                ToVector3(_startPos),
                ToVector3(right),
                ToVector3(bottom),
                ToVector3(left),
                ToVector3(_endPos)
            };

            Handles.color = Color.grey;
            Handles.DrawAAPolyLine(5f, _linePoints);
            EditorUtilities.DrawArrow(right, bottom, false);

            _hasLine = true;
        }

        private void DrawArrow()
        {
            if (SourceNodeModel != null &&
                TargetNodeModel != null &&
                SourceNodeModel.GetTransitionCount(TargetNodeModel) > 1)
            {
                EditorUtilities.DrawTrippleArrow(_startPos, _endPos, false);
            }
            else
            {
                EditorUtilities.DrawArrow(_startPos, _endPos, false);
            }
        }

        private Vector2 GetReciprocalOffset(Vector2 direction)
        {
            if (!HasReciprocalTransition())
                return Vector2.zero;

            var perpendicular = new Vector2(-direction.y, direction.x);
            return perpendicular * ReciprocalConnectionOffset;
        }

        private bool HasReciprocalTransition()
        {
            return SourceNodeModel != null &&
                   TargetNodeModel != null &&
                   SourceNodeModel != TargetNodeModel &&
                   TargetNodeModel.GetTransitionCount(SourceNodeModel) > 0;
        }

        private static Vector2 GetRectEdgePoint(Rect rect, Vector2 origin, Vector2 direction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                return rect.center;

            direction.Normalize();
            origin = ClampToRect(origin, rect);

            var xScale = float.PositiveInfinity;
            if (Mathf.Abs(direction.x) > Mathf.Epsilon)
            {
                var xEdge = direction.x > 0f ? rect.xMax : rect.xMin;
                xScale = (xEdge - origin.x) / direction.x;
            }

            var yScale = float.PositiveInfinity;
            if (Mathf.Abs(direction.y) > Mathf.Epsilon)
            {
                var yEdge = direction.y > 0f ? rect.yMax : rect.yMin;
                yScale = (yEdge - origin.y) / direction.y;
            }

            var scale = Mathf.Min(xScale, yScale);
            return float.IsInfinity(scale) ? origin : origin + direction * Mathf.Max(0f, scale);
        }

        private static Vector2 ClampToRect(Vector2 point, Rect rect)
        {
            return new Vector2(
                Mathf.Clamp(point.x, rect.xMin, rect.xMax),
                Mathf.Clamp(point.y, rect.yMin, rect.yMax));
        }

        private static Vector3 ToVector3(Vector2 value)
        {
            return new Vector3(value.x, value.y, 0f);
        }

        private static float DistanceToPolyLine(Vector3 mousePos, params Vector3[] points)
        {
            if (points == null || points.Length < 2)
                throw new ArgumentNullException(nameof(points));
            float dist = HandleUtility.DistancePointToLineSegment(mousePos, points[0], points[1]);
            for (int i = 2; i < points.Length; i++)
            {
                float d = HandleUtility.DistancePointToLineSegment(mousePos, points[i - 1], points[i]);
                if (d < dist)
                    dist = d;
            }
            return dist;
        }

        public void Draw()
        {
            DrawConnection();
        }

        internal void DrawDebugOverlay(Actor actor, string graphName)
        {
            if (!_hasLine)
                return;

            StateMachineDebugOverlay.DrawConnectionOverlay(this, actor, graphName, _startPos, _endPos, _isInverted);
        }

        public void ProcessMouseEvent(Event e, bool isReadOnlyMode)
        {
            if (!_hasLine || _linePoints == null)
                return;

            ProcessMouseEvent(e, isReadOnlyMode, _linePoints);
        }

        private void ProcessMouseEvent(Event e, bool isReadOnlyMode, Vector3[] points)
        {
            if (DistanceToPolyLine(e.mousePosition, points) < 10)

                switch (e.type)
                {
                    case EventType.MouseDown:

                        if (e.button == 0)
                        {
                            StateTransitionInspector.SelectedTransitionIndex = SourceNodeModel.GetTransitionStateIndex(TargetNodeModel);
                            Selection.activeObject = SourceNodeModel.GetTransitionStateModel(TargetNodeModel);
                            StartNode.IsFocused = false;
                            e.Use();
                        }
                        break;
                    case EventType.MouseUp:
                        if (e.button == 1)
                        {
                            if (isReadOnlyMode)
                                return;

                            e.Use();
                            ProcessContextMenu(e.mousePosition);
                        }
                        break;

                }
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Clear Connection"), false, () => _removeConnection?.Invoke(this));
            genericMenu.ShowAsContext();
        }
    }
}
