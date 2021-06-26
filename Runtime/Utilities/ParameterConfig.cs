using SAS.Utilities;
using System;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    [CreateAssetMenu(menuName = "SAS/State Machine Graph/Parameters Config (Animator/Actor)")]
    internal class ParameterConfig : ScriptableObject
    {
        [System.Serializable]
        internal class ParametersKeyMap
        {
            public string key;
            public Parameter[] parameters;
            public bool executeOnStateEnter = default;
            public bool executeOnStateExit = default;
        }

        [SerializeField] private ParametersKeyMap[] m_ParametersKeyMap;

        internal ParametersKeyMap Get(string key)
        {
            return Array.Find(m_ParametersKeyMap, ele => ele.key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

    }
}
