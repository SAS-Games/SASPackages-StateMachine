using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraphEditor
{
    public class StateTransitionEditor
    {
        private List<Connection> _transitions = new List<Connection>();
        private Port _endPort;
        private Port _startPort;
        SerializedObject _stateMachineSO;
        public StateTransitionEditor(SerializedObject stateMachineSO)
        {
            _stateMachineSO = stateMachineSO;
        }

        public void DrawConnections()
        {
            if (_transitions != null)
            {
                for (int i = 0; i < _transitions.Count; i++)
                    _transitions[i].Draw();
            }
        }

        public void DrawConnectionLine(Event e)
        {
            if (_endPort != null && _startPort == null)
            {
                Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 3, new Vector3[] { _endPort.rect.center, e.mousePosition });
                GUI.changed = true;
            }

            if (_startPort != null && _endPort == null)
            {
                Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, 3, new Vector3[] { _startPort.rect.center, e.mousePosition });
                GUI.changed = true;
            }
        }

        public void StartTransition(Node node)
        {
            ClearConnectionSelection();
            _startPort = node.startPort;
        }

        public void MakeTransion(Node node)
        {
            if (_startPort != null)
            {
                _endPort = node.endPort;
                if (_startPort.node != _endPort.node)
                {
                    AddTransition();
                    ClearConnectionSelection();
                }
                else
                    ClearConnectionSelection();
            }
        }

        public void Add(Port start, Port end)
        {
            _transitions.Add(new Connection(_stateMachineSO, start, end, RemoveTransition));
            start.node.SwapPort(false);
            end.node.SwapPort(true);
        }

        private void AddTransition()
        {
            _transitions.Add(new Connection(_stateMachineSO, _startPort, _endPort, RemoveTransition));
            AddStateTransition(_startPort.node, _endPort.node);
            _startPort.node.SwapPort(false);
            _endPort.node.SwapPort(true);
        }

        private void RemoveTransition(Connection connection)
        {
            // connection.startPort.node.state.RemoveTransitionState(connection.endPort.node.state);
            _transitions.Remove(connection);
        }

        public void ClearNodeConnection(Node node)
        {
            if (_transitions != null)
            {
                List<Connection> connectionsToRemove = new List<Connection>();

                for (int i = 0; i < _transitions.Count; i++)
                {
                    if (_transitions[i].endPort == node.endPort || _transitions[i].startPort == node.startPort)
                        connectionsToRemove.Add(_transitions[i]);
                }

                for (int i = 0; i < connectionsToRemove.Count; i++)
                    _transitions.Remove(connectionsToRemove[i]);
            }
        }

        public void ClearConnectionSelection()
        {
            _endPort = null;
            _startPort = null;
        }

        private void AddStateTransition(Node sourceStateNode, Node targerStateNode)
        {
            var stateModelSO = sourceStateNode.stateModelSO;
            var stateTranstionsList = stateModelSO.FindProperty("m_Transitions");
            stateTranstionsList.InsertArrayElementAtIndex(stateTranstionsList.arraySize);
            var transitionState = stateTranstionsList.GetArrayElementAtIndex(stateTranstionsList.arraySize-1);
            var targetState = transitionState.FindPropertyRelative("m_TargetState");
            targetState.objectReferenceValue = targerStateNode.stateModelSO.targetObject;
            var conditions = transitionState.FindPropertyRelative("m_Conditions");
            conditions.arraySize = 0;
            stateTranstionsList.serializedObject.ApplyModifiedProperties();
            stateModelSO.ApplyModifiedProperties();
        }
    }
}
