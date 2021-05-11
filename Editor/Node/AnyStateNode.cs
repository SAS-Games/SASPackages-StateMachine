using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class AnyStateNode : BaseNode
    {
        private Action<BaseNode> _startTransition;
        private StateMachineModel _stateMachineModel;

        public AnyStateNode(StateMachineModel stateMachineModel, SerializedObject serializedObject, Vector2 position, Action<BaseNode> startTransition) :
               base(serializedObject, position, Settings.AnyStateNodeStyle, Settings.AnyStateFocusedNodeStyle)
        {
            _stateMachineModel = stateMachineModel;
            _startTransition = startTransition;
        }

        protected override void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Make Transition"), false, () => _startTransition?.Invoke(this));
            genericMenu.ShowAsContext();
        }

        protected override void ProcessMouseUp(BaseNode baseNode)
        {
        }

        public override void Drag(Vector2 delta)
        {
            base.Drag(delta);
            _stateMachineModel.SetAnyStatePosition((serializedObject.targetObject as StateModel).GetPosition()); ;
        }
    }
}
