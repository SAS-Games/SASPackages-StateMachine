using System;

namespace SAS.StateMachineGraph.Utilities
{
    public class ObserveChildActorStateEnter : IAwaitableStateAction
    {
        public bool IsCompleted { get; set; }
        private Actor[] _childActors;
        private string _childStateTag;
        private int actorsStateEnterCount;

        public void OnInitialize(Actor actor, string tag, string key, State state)
        {
            _childStateTag = key;
            actor.TryGetComponentsInChildren(out _childActors, tag, true);
        }

        public void Execute(Actor actor)
        {
            IsCompleted = false;
            actorsStateEnterCount = 0;
            ChildActorsStateEnter(_childActors);
        }

        private void ChildActorsStateEnter(Actor[] _childActors)
        {
            foreach (var actor in _childActors)
            {
                Action<State> stateEnterEvent = null;
                stateEnterEvent = state =>
                {
                    if (state.Tag.Equals(_childStateTag))
                    {
                        actorsStateEnterCount++;
                        IsCompleted = actorsStateEnterCount == _childActors.Length;
                        actor.OnStateEnter -= stateEnterEvent;
                    }
                };

                actor.OnStateEnter += stateEnterEvent;
            }
        }
    }
}
