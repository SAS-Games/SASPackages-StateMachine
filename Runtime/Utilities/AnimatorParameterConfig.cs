using UnityEngine;
using System;

namespace SAS.StateMachineGraph.Utilities
{
    [CreateAssetMenu(menuName = "SAS/State Machine Character Controller/Animator Parameters Config")]
    public class AnimatorParameterConfig : ScriptableObject
    {
        [System.Serializable]
        public class ParametersKeyMap
        {
            public string key;
            public Parameter[] parameters;
            public string awaitableStateTag;
            public float startDelay;
        }

        [SerializeField] private ParametersKeyMap[] m_ParametersKeyMap;

#if UNITY_EDITOR
        [SerializeField, TextArea] private string m_Description;
#endif

        public bool TryGet(string key, out ParametersKeyMap parametersKeyMap)
        {
            parametersKeyMap = Array.Find(m_ParametersKeyMap, ele => ele.key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (parametersKeyMap == null)
            {
                Debug.LogWarning($"No parameters has been found wrt the key:  {key} under the Parameter config SO : {this.name}");
                return false;
            }
            return true;
        }

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
