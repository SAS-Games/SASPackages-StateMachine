using System;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public class ObserveChildActorStateEnter : IAwaitableStateAction
    {
        private Actor _childActor;
        public bool IsCompleted { get; set; }
        private State _state;
        private Action<State> _onStateEnter;

        public void OnInitialize(Actor actor, string tag, string key, State state)
        {
            _state = state;
            actor.TryGetComponentInChildren(out _childActor, tag, true);
        }

        public void Execute(Actor actor)
        {
            IsCompleted = false;
            _onStateEnter = (state) =>
            {
                if (state.Equals(_state))
                {
                    IsCompleted = true;
                    _childActor.OnStateEnter -= _onStateEnter;
                }
            };

            _childActor.OnStateEnter += _onStateEnter;
        }
    }
}
