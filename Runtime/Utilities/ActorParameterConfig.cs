using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    [CreateAssetMenu(menuName = "SAS/State Machine Character Controller/Actor Parameters Config)")]
    internal class ActorParameterConfig : ParameterConfigBase
    {
        public void ApplyParameters(in Actor actor, in string key)
        {
            if (TryGet(key, out var parametersKeyMap))
            {
                for (int i = 0; i < parametersKeyMap.parameters.Length; ++i)
                    actor.Apply(parametersKeyMap.parameters[i]);
            }
        }
    }
}
