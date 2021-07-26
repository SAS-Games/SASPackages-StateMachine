using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SAS.StateMachineGraph.Editor
{
    public class StateNode : BaseNode
    {
        protected Action<BaseNode> _startTransition;
        protected Action<BaseNode> _createConnection;
        private Action<StateNode, bool> _setAsDefaultNode;
        protected Action<StateNode> _removeNode;
        private Action<StateNode> _duplicateNode;

        internal StateModel Value => TargetObject as StateModel;
        private bool _isDefaultNode;
       
        public StateNode(Object targetObject, Vector2 position, bool isDefaultState, Action<BaseNode> startTransition, Action<BaseNode> makeTransition, Action<StateNode> removeNode, Action<StateNode, bool> setAsDefaultNode, Action<StateNode> duplicateNode) :
              base(targetObject, position)
        {
            _removeNode = removeNode;
            _startTransition = startTransition;
            _setAsDefaultNode = setAsDefaultNode;
            _createConnection = makeTransition;
            _duplicateNode = duplicateNode;
            _isDefaultNode = isDefaultState;
            
            if (_isDefaultNode)
            {
                _normalStyleName = "flow node 5";
                _focusedStyleName = "flow node 5 on";
            }
        }

        public void SetDefault(bool isDefault)
        {
            _isDefaultNode = isDefault;
            _normalStyleName = isDefault ? "flow node 5" : "flow node 0";
            _focusedStyleName = isDefault ? "flow node 5 on" : "flow node 0 on";
        }

        protected override void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Make Transition"), false, () => _startTransition.Invoke(this));
            if (!_isDefaultNode)
                genericMenu.AddItem(new GUIContent("Set as Default State"), false, () => _setAsDefaultNode.Invoke(this, true));
            genericMenu.AddItem(new GUIContent("Delete"), false, () => _removeNode.Invoke(this));
            genericMenu.AddItem(new GUIContent("Duplicate"), false, () => _duplicateNode.Invoke(this));
            genericMenu.ShowAsContext();
        }

        protected override void ProcessMouseUp(BaseNode baseNode, Event e)
        {
            _createConnection.Invoke(this);
        }
    }
}
