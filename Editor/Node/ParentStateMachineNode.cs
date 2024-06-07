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
        public ParentStateMachineNode(Object targetObject, Vector2Int position, bool isDefault, Action<StateMachineNode, StateModel> makeTransition, Action<StateMachineNode> mouseup, Action<StateMachineNode> selectStateMachine) :
                                 base(targetObject, position, isDefault, makeTransition, mouseup, null, selectStateMachine, null)
        {
            Prefix = "(Up)";
        }

        protected override void ProcessOnDoubleClicked(BaseNode baseNode)
        {
            _selectStateMachine.Invoke(this);
        }

        protected override void ProcessContextMenu()
        {

        }

        public override Vector3Int Position
        {
            get { return Value.serializedObject().FindProperty("m_PositionAsUpNode").vector3IntValue; }
            protected set
            {
                var serializedObject = Value.serializedObject();
                serializedObject.FindProperty("m_PositionAsUpNode").vector3IntValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
