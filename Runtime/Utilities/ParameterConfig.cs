using System;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    [CreateAssetMenu(menuName = "SAS/State Machine Character Controller/Parameters Config (Animator|Actor)")]
    internal class ParameterConfig : ScriptableObject
    {
        [System.Serializable]
        internal class ParametersKeyMap
        {
            public string key;
            public Parameter[] parameters;
        }

        [SerializeField] private ParametersKeyMap[] m_ParametersKeyMap;

        internal ParametersKeyMap Get(string key)
        {
            return Array.Find(m_ParametersKeyMap, ele => ele.key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

    }
}
