using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SAS.StateMachineGraph.Editor
{
    public sealed class EntryStateNode : BaseNode
    {
        private readonly Action<BaseNode> _startTransition;

        internal EntryStateModel Value => TargetObject as EntryStateModel;

        public EntryStateNode(Object targetObject, Vector2Int position, Action<BaseNode> startTransition)
            : base(targetObject, position, 120, 40)
        {
            _normalStyleName = "flow node 1";
            _focusedStyleName = "flow node 1 on";
            Prefix = "";
            _startTransition = startTransition;
        }

        protected override void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Make Transition"), false, () => _startTransition.Invoke(this));
            genericMenu.ShowAsContext();
        }

        protected override void ProcessMouseUp(BaseNode baseNode, Event e)
        {
        }
    }
}
