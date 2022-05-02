using System;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    public abstract class ParameterConfigBase : ScriptableObject
    {
        [System.Serializable]
        protected class ParametersKeyMap
        {
            public string key;
            public Parameter[] parameters;
        }

        [SerializeField] private ParametersKeyMap[] m_ParametersKeyMap;
#if UNITY_EDITOR
        [SerializeField, TextArea] private string m_Description;
#endif

        protected bool TryGet(string key, out ParametersKeyMap parametersKeyMap)
        {
            parametersKeyMap = Array.Find(m_ParametersKeyMap, ele => ele.key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (parametersKeyMap == null)
            {
                Debug.LogWarning($"No parameters has been found wrt the key:  {key} under the Parameter config SO : {this.name}");
                return false;
            }
            return true;
        }
    }
}
