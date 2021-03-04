using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public class ApplyActorParameter : IStateInitialize, IStateEnter
    {
        private Actor[] _actors;
        private Parameter _parameter;
        public void OnInitialize(Actor actor, string tag, string key)
        {
            actor.Get<ParameterConfig>(tag).TryGet(key, out _parameter);
            
            if (!actor.TryGetAll(tag, out _actors, true))
                Debug.LogError($"No Actor with tag {tag} is found usder {actor}. Try assigning the Tag");
        }

        public void OnStateEnter(Actor actor)
        {
            foreach (var childActor in _actors)
                childActor.Apply(in _parameter);
        }
    }
}
