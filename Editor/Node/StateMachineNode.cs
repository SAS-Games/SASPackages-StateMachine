using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace SAS.StateMachineGraph.Editor
{
    public class StateMachineNode : BaseNode
    {
        private Action<BaseNode> _startTransition;
        private Action<StateModel> _createConnection;
        private Action<StateMachineNode> _removeNode;
        private Action<StateMachineNode> _selectStateMachine;
        public StateMachineModel Value { get; }

        public StateMachineNode(SerializedObject serializedObject, Vector2 position, Action<BaseNode> startTransition, Action<StateModel> makeTransition, Action<StateMachineNode> removeNode, Action<StateMachineNode> selectStateMachine) :
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

        }

        protected override void ProcessOnDoubleClicked(BaseNode baseNode)
        {
            _selectStateMachine.Invoke(this);
        }

        private void CreateContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            CreateGenericMenuItems(genericMenu);
            genericMenu.ShowAsContext();
        }

        private void CreateGenericMenuItems(GenericMenu genericMenu)
        {
            Queue<string> traversedPath = new Queue<string>();
            Queue<StateMachineModel> stateMachineModels = new Queue<StateMachineModel>();
            stateMachineModels.Enqueue(Value);
            traversedPath.Enqueue("States");

            while (stateMachineModels.Count != 0)
            {
                var stateMachinModel = stateMachineModels.Dequeue();
                var curPath = traversedPath.Dequeue();
                var states = stateMachinModel.GetStates();

                foreach (var state in states)
                    genericMenu.AddItem(new GUIContent(curPath + "/" + state.name), false, OnConnectEndStateSelected, state);

                foreach (var childSMM in stateMachinModel.GetChildStateMachines())
                {
                    stateMachineModels.Enqueue(childSMM);
                    traversedPath.Enqueue(curPath + "/" + childSMM.name);
                }
            }
        }

        private void OnConnectEndStateSelected(object stateModel)
        {
            _createConnection((StateModel)stateModel);
        }
    }
}
