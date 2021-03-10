using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    [CreateAssetMenu(menuName = "SAS/State Machine Controller/ScriptableParameter")]
    public class ScriptableStateMachineParameter : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private Parameter m_InitialValue = default;
        [NonSerialized] public Parameter runtimeValue;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
           // throw new NotImplementedException();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            runtimeValue = m_InitialValue;
        }
    }
}
