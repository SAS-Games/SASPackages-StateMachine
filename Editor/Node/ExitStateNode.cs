using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SAS.StateMachineGraph.Editor
{
    public sealed class ExitStateNode : BaseNode
    {
        private readonly Action<BaseNode> _createConnection;

        internal ExitStateModel Value => TargetObject as ExitStateModel;

        public ExitStateNode(Object targetObject, Vector2Int position, Action<BaseNode> makeTransition)
            : base(targetObject, position, 120, 40)
        {
            _normalStyleName = "flow node 6";
            _focusedStyleName = "flow node 6 on";
            Prefix = "";
            _createConnection = makeTransition;
        }

        protected override void ProcessContextMenu()
        {
        }

        protected override void ProcessMouseUp(BaseNode baseNode, Event e)
        {
            _createConnection.Invoke(this);
        }
    }
}
