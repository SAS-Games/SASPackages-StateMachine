using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SAS.StateMachineGraph.Editor
{
    public class DefaultStateNode : StateNode
    {

        public DefaultStateNode(Object targetObject, Vector2 position, Action<BaseNode> startTransition, Action<BaseNode> makeTransition, Action<StateNode> removeNode) :
              base(targetObject, position, startTransition, makeTransition, removeNode, null)
        {
            _normalStyleName = "flow node 5";
            _focusedStyleName = "flow node 5 on";
        }

        protected override void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Make Transition"), false, () => _startTransition.Invoke(this));
            genericMenu.AddItem(new GUIContent("Delete"), false, () => _removeNode.Invoke(this));
            genericMenu.ShowAsContext();
        }
    }
}
