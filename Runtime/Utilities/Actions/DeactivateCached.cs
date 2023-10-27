using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.Scripting;

namespace SAS.StateMachineGraph.Utilities
{
    [Preserve]
    public sealed class DeactivateCached : IStateAction
    {
        IActivatable[] _activatables;
        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            if (!actor.TryGetComponentsInChildren(out _activatables, tag, true))
            {
                Debug.LogError($"No Activatable Component found with tag {tag} is found usder {actor}. Try assigning the Tag");
                return;
            }
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            foreach (IActivatable activatable in _activatables)
                activatable.Deactivate();
        }
    }
}
