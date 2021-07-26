using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class DeactivateObjects: IStateAction
    {
        private IActivatable[] _activatables;
        
        void IStateAction.OnInitialize(Actor actor, string tag, string key, State state)
        {
            if (!actor.GetComponentsInChildren(tag, out _activatables, true))
                Debug.LogError($"No GameObject with tag {tag} is found usder {actor}. Try assigning the Tag");
        }

        void IStateAction.Execute(Actor actor)
        {
            foreach (IActivatable activatable in _activatables)
                activatable.Deactivate();
        }
    }
}
