using System;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class Connection
    {
        public BaseNode StartNode { get; }
        public BaseNode EndNode { get; }

        private StateModel _sourceStateModel;
        private StateModel _targetStateModel;

        private Action<Connection> OnClickRemoveConnection;
        RuntimeStateMachineController _runtimeStateMachineController;

        public Connection(RuntimeStateMachineController runtimeStateMachineController, BaseNode start, BaseNode end, StateModel sourceStateModel, StateModel targetStateModel, Action<Connection> onClickRemoveConnection)
        {
            StartNode = start;
            EndNode = end;
            _sourceStateModel = sourceStateModel;
            _targetStateModel = targetStateModel;
            _runtimeStateMachineController = runtimeStateMachineController;
        }

        private void DrwaConnection(Event e)
        {
            Vector2 startPos;
            Vector2 endPos;
            bool inverted = false;
            if (StartNode.Position.y < EndNode.Position.y)
            {
                startPos = StartNode.startPort.rect.center;
                endPos = EndNode.startPort.rect.center;
            }
            else
            {
                startPos = EndNode.endPort.rect.center;
                endPos = StartNode.endPort.rect.center;
                inverted = true;
            }

            EditorUtilities.DrawLine(startPos, endPos);
            EditorUtilities.DrawArrow(startPos, endPos, inverted);
            ProcessMouseEvent(e, new Vector3[] { startPos, endPos });
        }

        private static float DistanceToPolyLine(params Vector3[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));
            float dist = HandleUtility.DistanceToLine(points[0], points[1]);
            for (int i = 2; i < points.Length; i++)
            {
                float d = HandleUtility.DistanceToLine(points[i - 1], points[i]);
                if (d < dist)
                    dist = d;
            }
            return dist;
        }

        public void Draw(Event e)
        {
            DrwaConnection(e);
        }

        private void ProcessMouseEvent(Event e, Vector3[] points)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (DistanceToPolyLine(points) < 10)
                        {
                            Selection.activeObject = _sourceStateModel;
                            StartNode.IsFocused = false;
                            int index = _sourceStateModel.GetTransitionStateIndex(_targetStateModel); //ToDo: will visit it later
                            StateTransitionInspector.Show(index, _runtimeStateMachineController, _sourceStateModel.ToSerializedObject());
                        }
                    }
                    break;
            }
        }
    }
}