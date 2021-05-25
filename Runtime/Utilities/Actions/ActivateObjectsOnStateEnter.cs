using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class ActivateObjectsOnStateEnter : IStateInitialize, IStateEnter
    {
        private Transform _transform;
        void IStateInitialize.OnInitialize(Actor actor, string tag, string key)
        {
            actor.TryGetComponentInChildren(out _transform, tag, true);
        }

        void IStateEnter.OnStateEnter(Actor actor)
        {
            _transform?.gameObject.SetActive(true);
        }
    }
}
