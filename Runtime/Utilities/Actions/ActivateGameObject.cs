using SAS.Utilities.TagSystem;
using UnityEngine;
using UnityEngine.Scripting;

namespace SAS.StateMachineGraph.Utilities
{
    [Preserve]
    public sealed class ActivateGameObject : IStateAction
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
            _actor.TryGetComponentInChildren(out Transform transform, _tag, true);
            transform?.gameObject.SetActive(true);
        }
    }
}
