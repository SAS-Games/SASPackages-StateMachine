using SAS.Core.TagSystem;
using UnityEngine;
using UnityEngine.Scripting;

namespace SAS.StateMachineGraph.Utilities
{
    [Preserve]
    public sealed class DeactivateGameObject : IStateAction
    {
        private Actor _actor;
        private Tag _tag;
        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            _actor = actor;
            _tag = tag;
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            _actor.TryGetComponentInChildren(out Transform _transform, _tag, true);
            _transform?.gameObject.SetActive(false);
        }
    }
}
