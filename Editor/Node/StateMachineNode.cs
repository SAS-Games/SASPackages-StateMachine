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
    public class StateMachineNode : BaseNode
    {
        private Action<StateMachineNode, StateModel> _createConnection;
        private Action<StateMachineNode> _mouseup;
        private Action<StateMachineNode> _removeNode;
        private Action<StateMachineNode> _selectStateMachine;

        public StateMachineModel Value => TargetObject as StateMachineModel;

        public StateMachineNode(Object targetObject, Vector2 position, Action<StateMachineNode, StateModel> makeTransition, Action<StateMachineNode> mouseup,  Action<StateMachineNode> removeNode, Action<StateMachineNode> selectStateMachine) :
            base(targetObject, position, 150, 100, Settings.NodeNormalStyle, Settings.NodeFocudeStyle)
        {
            _removeNode = removeNode;
            _mouseup = mouseup;
            _createConnection = makeTransition;
            _selectStateMachine = selectStateMachine;
        }

        protected override void ProcessContextMenu()
        {
             GenericMenu genericMenu = new GenericMenu();
             genericMenu.AddItem(new GUIContent("Delete"), false, () => _removeNode.Invoke(this));
             genericMenu.ShowAsContext();
        }

        protected override void ProcessMouseUp(BaseNode baseNode, Event e)
        {
            e.Use();
             _mouseup.Invoke(this);
        }

        protected override void ProcessOnDoubleClicked(BaseNode baseNode)
        {
            _selectStateMachine.Invoke(this);
        }

        public bool CreateAvailableStatesGenericMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            CreateGenericMenuItems(genericMenu);
            genericMenu.ShowAsContext();
            return genericMenu.GetItemCount() > 0;
        }

        private void CreateGenericMenuItems(GenericMenu genericMenu)
        {
            Queue<string> traversedPath = new Queue<string>();
            Queue<StateMachineModel> stateMachineModels = new Queue<StateMachineModel>();
            stateMachineModels.Enqueue(TargetObject as StateMachineModel);
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
            _createConnection(this, (StateModel)stateModel);
        }
    }
}
