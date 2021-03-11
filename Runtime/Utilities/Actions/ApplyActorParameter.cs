using UnityEngine;
using SAS.ScriptableTypes;

namespace SAS.StateMachineGraph.Utilities
{
    public class ApplyActorParameter : IStateInitialize, IStateEnter
    {
        private Actor[] _actors;
        private string _parameterKey;
        public void OnInitialize(Actor actor, string tag, string key)
        {
            _parameterKey = key;
            if (!actor.TryGetAll(tag, out _actors, true))
                Debug.LogError($"No Actor with tag {tag} is found usder {actor}. Try assigning the Tag");
        }

        public void OnStateEnter(Actor actor)
        {
            foreach (var cActor in _actors)
                cActor.Apply(cActor.Get<ScriptableParameter>(_parameterKey).runtimeValue);
        }
    }
}
