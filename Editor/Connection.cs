using System;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class Connection
    {
        public Node StartNode { get; }
        public Node EndNode { get; }
        private Action<Connection> OnClickRemoveConnection;
        private SerializedObject _stateMachineSO;

        public Connection(SerializedObject stateMachineSO, Node start, Node end, Action<Connection> onClickRemoveConnection)
        {
            StartNode = start;
            EndNode = end;
            _stateMachineSO = stateMachineSO;
            OnClickRemoveConnection = onClickRemoveConnection;
        }

        void DrwaConnection()
        {
            Vector2 startPos;
            Vector2 endPos;
            if (StartNode.GetPosition().y < EndNode.GetPosition().y)
            {
                startPos = StartNode.startPort.rect.center;
                endPos = EndNode.startPort.rect.center;
                Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 3, new Vector3[] { startPos, endPos });
            }
            else
            {
                startPos = EndNode.endPort.rect.center;
                endPos = StartNode.endPort.rect.center;
                Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 3, new Vector3[] { startPos, endPos });
            }
            DrawArrow(startPos, endPos);
        }

        private void DrawArrow(Vector2 startPos, Vector2 endPos)
        {
            Vector2 pos = startPos + (endPos - startPos).normalized * Vector2.Distance(startPos, endPos) * 0.5f;

            Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(Angle(StartNode.GetPosition(), EndNode.GetPosition()), pos);
            if (GUI.Button(new Rect(new Vector2(pos.x - 12, pos.y - 12), Vector2.one * 24), Settings.GetArrowTexture(), GUIStyle.none))
            {
                Selection.activeObject = null;
                Selection.activeObject = StartNode.stateModelSO.targetObject;
                StartNode.SetActvieStyle(false);
                int index = StartNode.state.GetTransitionStateIndex(EndNode.state);
                StateTransitionInspector.Show(index, _stateMachineSO, StartNode.stateModelSO);
            }
            GUI.matrix = matrixBackup;
        }

        public void Draw()
        {
            DrwaConnection();
        }

        public static float Angle(Vector2 from, Vector2 to)
        {
            return Mathf.Atan2(to.y - from.y, to.x - from.x) * 180f / Mathf.PI;
        }
    }
}