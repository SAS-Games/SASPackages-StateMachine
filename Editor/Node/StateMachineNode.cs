using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class StateMachineNode : BaseNode
    {
        private Action<BaseNode> _startTransition;
        private Action<BaseNode> _createConnection;
        private Action<StateMachineNode> _removeNode;
        private Action<StateMachineNode> _selectStateMachine;
        public StateMachineModel Value { get; }

        public StateMachineNode(SerializedObject serializedObject, Vector2 position, Action<BaseNode> startTransition, Action<BaseNode> makeTransition, Action<StateMachineNode> removeNode, Action<StateMachineNode> selectStateMachine) :
            base(serializedObject, position, 150, 100, Settings.NodeNormalStyle, Settings.NodeFocudeStyle)
        {
            Value = serializedObject.targetObject as StateMachineModel;
            _removeNode = removeNode;
            _startTransition = startTransition;
            _createConnection = makeTransition;
            _selectStateMachine = selectStateMachine;
        }

        protected override void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Make Transition"), false, () => _startTransition.Invoke(this));
            genericMenu.AddItem(new GUIContent("Delete"), false, () => _removeNode.Invoke(this));
            genericMenu.ShowAsContext();
        }

        protected override void ProcessMouseUp(BaseNode baseNode)
        {
            _createConnection.Invoke(this);
        }

        protected override void ProcessOnDoubleClicked(BaseNode baseNode)
        {
            _selectStateMachine.Invoke(this);
        }
    }
}
