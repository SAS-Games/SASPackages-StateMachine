using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;

namespace SAS.StateMachineGraph.Editor
{
    public class ParentStateMachineNode : StateMachineNode
    {
        private Action<StateMachineNode> _selectStateMachine;
        public ParentStateMachineNode(Object targetObject, Vector2 position, Action<StateMachineNode> selectStateMachine) : base(targetObject, position, null, null, null, selectStateMachine)
        {
            _selectStateMachine = selectStateMachine;
            Prefix = "(Up)";
        }

        protected override void ProcessOnDoubleClicked(BaseNode baseNode)
        {
            _selectStateMachine.Invoke(this);
        }

        protected override void ProcessContextMenu()
        {

        }

        protected override void ProcessMouseUp(BaseNode baseNode, Event e)
        {
            // throw new NotImplementedException();
        }
    }
}
