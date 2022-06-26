using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class ActivateObjects : IStateAction
    {
        private IActivatable[] _activatables;
        void IStateAction.OnInitialize(Actor actor, string tag, string key, State state)
        {
            if (!actor.TryGetComponentsInChildren(out _activatables, tag, true))
                Debug.LogError($"No GameObject with tag {tag} is found usder {actor}. Try assigning the Tag");
        }

        void IStateAction.Execute()
        {
            foreach (IActivatable activatable in _activatables)
                activatable.Activate();
        }
    }
}
