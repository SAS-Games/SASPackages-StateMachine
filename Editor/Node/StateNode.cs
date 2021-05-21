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

        internal StateModel Value => TargetObject as StateModel;
       
        public StateNode(Object targetObject, Vector2 position, Action<BaseNode> startTransition, Action<BaseNode> makeTransition, Action<StateNode> removeNode, Action<StateNode, bool> setAsDefaultNode) : 
            this(targetObject, position, startTransition,  makeTransition,  removeNode,  setAsDefaultNode, Settings.NodeNormalStyle, Settings.NodeFocudeStyle)
        {
        }

        protected StateNode(Object targetObject, Vector2 position, Action<BaseNode> startTransition, Action<BaseNode> makeTransition, Action<StateNode> removeNode, Action<StateNode, bool> setAsDefaultNode, GUIStyle normal, GUIStyle focussed) :
              base(targetObject, position, normal, focussed)
        {
            _removeNode = removeNode;
            _startTransition = startTransition;
            _setAsDefaultNode = setAsDefaultNode;
            _createConnection = makeTransition;
        }

        protected override void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Make Transition"), false, () => _startTransition.Invoke(this));
            genericMenu.AddItem(new GUIContent("Set as Default State"), false, () => _setAsDefaultNode.Invoke(this, true));
            genericMenu.AddItem(new GUIContent("Delete"), false, () => _removeNode.Invoke(this));
            genericMenu.ShowAsContext();
        }

        protected override void ProcessMouseUp(BaseNode baseNode, Event e)
        {
            _createConnection.Invoke(this);
        }
    }
}
