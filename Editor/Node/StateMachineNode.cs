using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SAS.StateMachineGraph.Editor
{
    public class StateMachineNode : BaseNode
    {
        private Action<BaseNode> _startTransition;
        private Action<BaseNode> _createConnection;
        private Action<StateMachineNode> _removeNode;
        protected Action<StateMachineNode> _selectStateMachine;
        private Action<StateMachineNode> _duplicateNode;

        public StateMachineModel Value => TargetObject as StateMachineModel;

        public StateMachineNode(Object targetObject, Vector2Int position, bool isDefault, Action<BaseNode> startTransition, Action<BaseNode> makeTransition, Action<StateMachineNode> removeNode, Action<StateMachineNode> selectStateMachine, Action<StateMachineNode> duplicateNode) :
            base(targetObject, position, 190, 40)
        {
            _normalStyleName = isDefault ? "flow node hex 5" : "flow node hex 0";
            _focusedStyleName = isDefault ? "flow node hex 5 on" : "flow node hex 0 on";
            _removeNode = removeNode;
            _startTransition = startTransition;
            _createConnection = makeTransition;
            _selectStateMachine = selectStateMachine;
            _duplicateNode = duplicateNode;
        }

        public void SetDefault(bool isDefault)
        {
            _normalStyleName = isDefault ? "flow node hex 5" : "flow node hex 0";
            _focusedStyleName = isDefault ? "flow node hex 5 on" : "flow node hex 0 on";
        }

        protected override void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Make Transition"), false, () => _startTransition.Invoke(this));
            genericMenu.AddItem(new GUIContent("Delete"), false, () => _removeNode?.Invoke(this));
            genericMenu.AddItem(new GUIContent("Duplicate"), false, () => _duplicateNode?.Invoke(this));
            AddDebugBreakpointMenu(genericMenu);
            genericMenu.ShowAsContext();
        }

        protected override void ProcessMouseUp(BaseNode baseNode, Event e)
        {
            e.Use();
            _createConnection.Invoke(this);
        }

        protected override void ProcessOnDoubleClicked(BaseNode baseNode)
        {
            _selectStateMachine.Invoke(this);
        }

    }
}
