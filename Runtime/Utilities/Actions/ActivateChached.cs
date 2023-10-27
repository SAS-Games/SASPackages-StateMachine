using SAS.Utilities.TagSystem;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class ActivateChached : IStateAction
    {
        private IActivatable[] _activatables;
        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            if (!actor.TryGetComponentsInChildren(out _activatables, tag, true))
                Debug.LogError($"No GameObject with tag {tag} is found usder {actor}. Try assigning the Tag");
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            foreach (IActivatable activatable in _activatables)
                activatable.Activate();
        }
    }
}
