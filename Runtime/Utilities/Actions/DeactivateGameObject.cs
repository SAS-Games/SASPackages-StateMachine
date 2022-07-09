using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class DeactivateGameObject : IStateAction
    {
        private Transform _transform;
        void IStateAction.OnInitialize(Actor actor, string tag, string key)
        {
            actor.TryGetComponentInChildren(out _transform, tag, true);
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            _transform?.gameObject.SetActive(false);
        }
    }
}
