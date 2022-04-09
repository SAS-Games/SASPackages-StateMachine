using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class ActivateGameObject : IStateAction
    {
        private Transform _transform;
        void IStateAction.OnInitialize(Actor actor, string tag, string key, State state)
        {
            Debug.Log("ActivateGameObject");
            actor.TryGetComponentInChildren(out _transform, tag, true);
        }

        void IStateAction.Execute(Actor actor)
        {
            _transform?.gameObject.SetActive(true);
        }
    }
}
