using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class StateTransitionEditor
    {
        private List<Connection> _transitions = new List<Connection>();
        private Port _endPort;
        private Port _startPort;

        public StateModel SourceStateModel { get; private set; }
        public StateModel TargetStateModel { get; private set; }
        RuntimeStateMachineController _runtimeStateMachineController;
        public StateTransitionEditor(RuntimeStateMachineController runtimeStateMachineController)
        {
            _runtimeStateMachineController = runtimeStateMachineController;
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
                EditorUtilities.DrawArrowLine(_endPort.rect.center, e.mousePosition);
                GUI.changed = true;
            }

            if (_startPort != null && _endPort == null)
            {
                EditorUtilities.DrawArrowLine(_startPort.rect.center, e.mousePosition);
                GUI.changed = true;
            }
        }

        public void Start(BaseNode node, StateModel sourceStateModel)
        {
            ClearConnectionSelection();
            _startPort = node.startPort;
            SourceStateModel = sourceStateModel;
        }

        public void Make(BaseNode node, StateModel targetStateModel)
        {
            TargetStateModel = targetStateModel;
            if (_startPort != null)
            {
                _endPort = node.endPort;
                AddTransition();
            }

            ClearConnectionSelection();
        }

        public void Add(BaseNode start, BaseNode end)
        {
            _transitions.Add(new Connection(_runtimeStateMachineController, start, end, RemoveTransition));
        }

        private void AddTransition()
        {
            _transitions.Add(new Connection(_runtimeStateMachineController, _startPort.node, _endPort.node, RemoveTransition));
            SourceStateModel.AddStateTransition(TargetStateModel);
        }

        private void RemoveTransition(Connection connection)
        {
            // connection.startPort.node.state.RemoveTransitionState(connection.endPort.node.state);
            _transitions.Remove(connection);
        }

        public void ClearNodeConnection(BaseNode node)
        {
            if (_transitions != null)
            {
                List<Connection> connectionsToRemove = new List<Connection>();

                for (int i = 0; i < _transitions.Count; i++)
                {
                    if (_transitions[i].EndNode == node || _transitions[i].StartNode == node)
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
            SourceStateModel = null;
            TargetStateModel = null;
        }
    }
}
