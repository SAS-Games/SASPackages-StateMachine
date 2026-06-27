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

        public TransitionNodeModel SourceNodeModel { get; private set; }
        public TransitionNodeModel TargetNodeModel { get; private set; }
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

        public void ProcessConnectionEvents(Event e)
        {
            if (_transitions != null)
            {
                for (int i = 0; i < _transitions.Count; i++)
                    _transitions[i].ProcessMouseEvent(e);
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

        public void Start(BaseNode node, TransitionNodeModel sourceNodeModel)
        {
            ClearConnectionSelection();
            _startPort = node.startPort;
            SourceNodeModel = sourceNodeModel;
        }

        public void Make(BaseNode node, TransitionNodeModel targetNodeModel)
        {
            TargetNodeModel = targetNodeModel;
            if (_startPort != null)
            {
                _endPort = node.endPort;
                AddTransition();
            }

            ClearConnectionSelection();
        }

        public void Add(BaseNode sourceNode, BaseNode targetNode, TransitionNodeModel sourceNodeModel, TransitionNodeModel targetNodeModel)
        {
            _transitions.Add(new Connection(sourceNode, targetNode, sourceNodeModel, targetNodeModel, RemoveTransition));
        }

        private void AddTransition()
        {
            _transitions.Add(new Connection(_startPort.node, _endPort.node, SourceNodeModel, TargetNodeModel, RemoveTransition));
            SourceNodeModel.AddStateTransition(_runtimeStateMachineController, TargetNodeModel);
        }

        private void RemoveTransition(Connection connection)
        {
            connection.SourceNodeModel.ClearConnection(connection.TargetNodeModel);
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
            SourceNodeModel = null;
            TargetNodeModel = null;
        }

        public void Clear()
        {
            _transitions.Clear();
            ClearConnectionSelection();
        }
    }
}
