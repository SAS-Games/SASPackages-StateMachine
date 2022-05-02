using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    [CreateAssetMenu(menuName = "SAS/State Machine Character Controller/Animator Parameters Config")]
    public class AnimatorParameterConfig : ParameterConfigBase
    {
        public void ApplyParameters(in Animator animator, in string key)
        {
            if (TryGet(key, out var parametersKeyMap))
            {
                for (int i = 0; i < parametersKeyMap.parameters.Length; ++i)
                    animator.Apply(parametersKeyMap.parameters[i]);
            }
        }
    }
}
