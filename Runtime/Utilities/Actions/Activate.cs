using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.Scripting;

namespace SAS.StateMachineGraph.Utilities
{
    [Preserve]
    public sealed class Activate : IStateAction
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
                Debug.LogError($"No GameObject with tag {_tag} is found usder {_actor}. Try assigning the Tag");
                return;
            }

            foreach (IActivatable activatable in activatables)
                activatable.Activate();
        }
    }
}
