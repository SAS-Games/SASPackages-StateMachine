using SAS.Utilities.TagSystem;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class Deactivate : IStateAction
    {
        private Actor _actor;
        private Tag _tag;
        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            _actor = actor;
            _tag = tag;
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            if (!_actor.TryGetComponentsInChildren(out IActivatable[] activatables, _tag, true))
            {
                Debug.LogError($"No Activatable Component found with tag {_tag} is found usder {_actor}. Try assigning the Tag");
                return;
            }
            foreach (IActivatable activatable in activatables)
                activatable.Deactivate();
        }
    }
}
