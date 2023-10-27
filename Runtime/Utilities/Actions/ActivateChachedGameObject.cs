using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.Scripting;

namespace SAS.StateMachineGraph.Utilities
{
    [Preserve]
    public sealed class ActivateChachedGameObject : IStateAction
    {
        private Transform _transform;
        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            actor.TryGetComponentInChildren(out _transform, tag, true);
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            _transform?.gameObject.SetActive(true);
        }
    }
}
