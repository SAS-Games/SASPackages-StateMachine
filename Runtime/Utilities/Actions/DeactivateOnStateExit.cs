using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class DeactivateOnStateExit: IStateInitialize, IStateExit
    {

        private IActivatable[] _activatables;
        void IStateInitialize.OnInitialize(Actor actor, string tag, string key)
        {
            if (!actor.TryGetAll(tag, out _activatables, true))
                Debug.LogError($"No GameObject with tag {tag} is found usder {actor}. Try assigning the Tag");
        }

        void IStateExit.OnStateExit(Actor actor)
        {
            foreach (IActivatable activatable in _activatables)
                activatable.Deactivate();
        }
    }
}
