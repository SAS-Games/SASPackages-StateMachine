using SAS.StateMachineGraph;
using System;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraphEditor
{
    public class Connection
    {
        public Port endPort;
        public Port startPort;
        private Action<Connection> OnClickRemoveConnection;
        private SerializedObject _stateMachineSO;

        public Connection(SerializedObject stateMachineSO, Port start, Port end, Action<Connection> onClickRemoveConnection)
        {
            endPort = end;
            startPort = start;
            _stateMachineSO = stateMachineSO;
            OnClickRemoveConnection = onClickRemoveConnection;
        }

        public void Draw()
        {
            Vector2 pos = startPort.rect.center + (endPort.rect.center -startPort.rect.center).normalized * Vector2.Distance(endPort.rect.center, startPort.rect.center) * 0.5f;
            
            Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 4, new Vector3[] { endPort.rect.center, startPort.rect.center });
            Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(Angle(startPort.rect.center, endPort.rect.center), pos);
            if (GUI.Button(new Rect(new Vector2(pos.x - 12, pos.y - 12), Vector2.one * 24), Settings.GetArrowTexture(), GUIStyle.none))
            {
                Selection.activeObject = null;
                Selection.activeObject = startPort.node.stateModelSO.targetObject;
                startPort.node.SetActvieStyle(false);
                int index = startPort.node.state.GetTransitionStateIndex(endPort.node.state);
                StateTransitionInspector.Show(index, _stateMachineSO, startPort.node.stateModelSO);
            }
            GUI.matrix = matrixBackup;

            //OnClickRemoveConnection?.Invoke(this);
        }

        public static float Angle(Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            return angle;
        }
    }
}