using System;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public class ObserveChildActorStateEnter : IStateInitialize, IAwaitableStateAction
    {
        private Actor _childActor;
        public bool IsCompleted { get; set; }
        private string _stateName;
        private Action<string> _onStateEnter;

        public void OnInitialize(Actor actor, string tag, string key)
        {
            _stateName = key;
            actor.TryGetComponentInChildren(out _childActor, tag, true);
        }

        public void Execute(Actor actor)
        {
            IsCompleted = false;
            _onStateEnter = (state) =>
            {
                if (state.Equals(_stateName))
                {
                    IsCompleted = true;
                    _childActor.OnStateEnter -= _onStateEnter;
                }
            };

            _childActor.OnStateEnter += _onStateEnter;
        }
    }
}
