using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public class ObserveChildActorStateExit : IStateInitialize, IAwaitableStateAction
    {
        private Actor _childActor;
        public bool IsCompleted { get; set; }
        private string _stateName;

        public void OnInitialize(Actor actor, string tag, string key)
        {
            _stateName = key;
            if (!actor.TryGet(tag, out _childActor, true))
                Debug.LogError($"No Actor with tag {tag} is found under {actor.name} attached on the object {actor.gameObject.name}. Try assigning the Tag");

            _childActor.OnStateExit += (state) =>
            {
                IsCompleted = state.Equals(_stateName);
            };
        }

        public void Execute(Actor actor)
        {
        }
    }
}
