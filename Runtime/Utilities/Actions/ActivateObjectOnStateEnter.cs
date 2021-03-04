using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class ActivateObjectOnStateEnter : IStateInitialize, IStateEnter
    {
        private Transform _transform;
        void IStateInitialize.OnInitialize(Actor actor, string tag, string key)
        {
                if (!actor.TryGet(tag, out _transform, true))
                    Debug.LogError($"No GameObject with tag {tag} is found usder {actor}. Try assigning the Tag");
        }

        void IStateEnter.OnStateEnter(Actor actor)
        {
            _transform?.gameObject.SetActive(true);
        }
    }
}
