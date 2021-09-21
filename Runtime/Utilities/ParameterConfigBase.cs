using System;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    internal abstract class ParameterConfigBase : ScriptableObject
    {
        [System.Serializable]
        internal class ParametersKeyMap
        {
            public string key;
            public Parameter[] parameters;
        }

        [SerializeField] private ParametersKeyMap[] m_ParametersKeyMap;
#if UNITY_EDITOR
        [SerializeField, TextArea] private string m_Description;
#endif

        internal ParametersKeyMap Get(string key)
        {
            return Array.Find(m_ParametersKeyMap, ele => ele.key.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

    }
}
