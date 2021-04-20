using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public sealed class ActivateObjectOnStateExit : IStateInitialize, IStateExit
    {
        private Transform _transform;
        void IStateInitialize.OnInitialize(Actor actor, string tag, string key)
        {
            actor.TryGet(out _transform, tag, true);
        }

       void IStateExit.OnStateExit(Actor actor)
        {
            _transform?.gameObject.SetActive(true);
        }
    }
}
