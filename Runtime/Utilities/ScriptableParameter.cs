using SAS.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    [CreateAssetMenu(menuName = "SAS/ScriptableTypes/FSMParameter")]
    public class ScriptableParameter : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private Parameter m_InitialValue = default;
        [NonSerialized] public Parameter runtimeValue;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            runtimeValue = m_InitialValue;
        }
    }
}
