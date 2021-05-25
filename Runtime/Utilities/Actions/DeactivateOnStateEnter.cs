using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class DeactivateOnStateEnter: IStateInitialize, IStateEnter
    {

        private IActivatable[] _activatables;
        void IStateInitialize.OnInitialize(Actor actor, string tag, string key)
        {
            if (!actor.GetComponentsInChildren(tag, out _activatables, true))
                Debug.LogError($"No GameObject with tag {tag} is found usder {actor}. Try assigning the Tag");
        }

        void IStateEnter.OnStateEnter(Actor actor)
        {
            foreach (IActivatable activatable in _activatables)
                activatable.Deactivate();
        }
    }
}
