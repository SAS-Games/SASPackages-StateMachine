using System;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class Connection
    {
        public BaseNode StartNode { get; }
        public BaseNode EndNode { get; }
        private Action<Connection> OnClickRemoveConnection;
        RuntimeStateMachineController _runtimeStateMachineController;

        public Connection(RuntimeStateMachineController runtimeStateMachineController, BaseNode start, BaseNode end, Action<Connection> onClickRemoveConnection)
        {
            StartNode = start;
            EndNode = end;
            _runtimeStateMachineController = runtimeStateMachineController;
           // OnClickRemoveConnection = onClickRemoveConnection;
        }

        void DrwaConnection()
        {
            Vector2 startPos;
            Vector2 endPos;
            if (StartNode.Position.y < EndNode.Position.y)
            {
                startPos = StartNode.startPort.rect.center;
                endPos = EndNode.startPort.rect.center;
            }
            else
            {
                startPos = EndNode.endPort.rect.center;
                endPos = StartNode.endPort.rect.center;
            }

            EditorUtilities.DrawArrowLine(startPos, endPos);
        }

        public static float DistanceToPolyLine(params Vector3[] points)
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

        private void DrawArrow(Vector2 startPos, Vector2 endPos)
        {
           // startPos = GUI.matrix.MultiplyVector(startPos);
           // endPos *= GUI.matrix.MultiplyVector(endPos);
           // Matrix4x4 matrixBackup = GUI.matrix;

            Vector2 pos = startPos + (endPos - startPos).normalized * Vector2.Distance(startPos, endPos) * 0.5f;
            Handles.Label(pos, " text");
            //  GUIUtility.RotateAroundPivot(Angle(StartNode.Position, EndNode.Position), pos);
            //  if (GUI.Button(new Rect(new Vector2(pos.x - 12, pos.y - 12), Vector2.one * 24), Settings.GetArrowTexture(), GUIStyle.none))
            {
               // Selection.activeObject = null;
              //  Selection.activeObject = StartNode.serializedObject.targetObject;
             //   StartNode.IsFocused = false;
               // int index = StartNode.state.GetTransitionStateIndex(EndNode.state); //ToDo: will visit it later
              //  StateTransitionInspector.Show(index, _stateMachineSO, StartNode.serializedObject);
            }
         //   GUI.matrix = matrixBackup;
        }

        public void Draw()
        {
            DrwaConnection();
        }
    }
}