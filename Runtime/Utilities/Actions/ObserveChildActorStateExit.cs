using System;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public class ObserveChildActorStateExit : IStateInitialize, IAwaitableStateAction
    {
        private Actor _childActor;
        public bool IsCompleted { get; set; }
        private string _stateName;
        private Action<string> _onStateExit;

        public void OnInitialize(Actor actor, string tag, string key)
        {
            _stateName = key;
            if (!actor.TryGet(tag, out _childActor, true))
                Debug.LogError($"No Actor with tag {tag} is found under {actor.name} attached on the object {actor.gameObject.name}. Try assigning the Tag");
        }

        public void Execute(Actor actor)
        {
            IsCompleted = false;
            _onStateExit = (state) =>
            {
                if (state.Equals(_stateName))
                {
                    IsCompleted = true;
                    _childActor.OnStateExit -= _onStateExit;
                }
            };

            _childActor.OnStateExit += _onStateExit;
        }
    }
}
