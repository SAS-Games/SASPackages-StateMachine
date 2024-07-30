using System;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [Serializable]
    public struct Condition
    {
        public enum Mode
        {
            If = 1,
            IfNot = 2,
            Greater = 3,
            Less = 4,
            Equals = 5,
            NotEqual = 6
        }

        [SerializeField] private string m_Name;
        [SerializeField] private Mode m_Mode;
        [SerializeField] internal StateMachineParameter.ParameterType m_Type;
        [SerializeField] private float m_FloatValue;
        [SerializeField] internal string m_CustomCondition;

        internal ICustomCondition Custom;
        private int _hashValue;

        internal bool IsValid(StateMachine stateMachine)
        {
            switch (m_Type)
            {
                case StateMachineParameter.ParameterType.Bool:
                    return stateMachine.GetBool(_hashValue) == (m_Mode == Mode.If);

                case StateMachineParameter.ParameterType.Int:
                    if (m_Mode == Mode.Greater)
                        return stateMachine.GetInteger(_hashValue) > (int)m_FloatValue;
                    else if (m_Mode == Mode.Less)
                        return stateMachine.GetInteger(_hashValue) < (int)m_FloatValue;
                    if (m_Mode == Mode.Equals)
                        return stateMachine.GetInteger(_hashValue) == (int)m_FloatValue;
                    return stateMachine.GetInteger(_hashValue) != (int)m_FloatValue;

                case StateMachineParameter.ParameterType.Float:
                    if (m_Mode == Mode.Greater)
                        return stateMachine.GetFloat(_hashValue) > m_FloatValue;
                    return stateMachine.GetFloat(_hashValue) < m_FloatValue;

                case StateMachineParameter.ParameterType.Trigger:
                    return stateMachine.GetBool(_hashValue) == true;
                case StateMachineParameter.ParameterType.Custom:
                    return Custom.Evaluate() == (m_Mode == Mode.If);
                default:
                    return false;
            }
        }

        public Condition Clone()
        {
            Condition clone = new Condition();
            clone.m_Name = m_Name;
            clone.m_Mode = m_Mode;
            clone.m_Type = m_Type;
            clone.m_FloatValue = m_FloatValue;
            clone._hashValue = Animator.StringToHash(m_Name);
            clone.m_CustomCondition = null;
            return clone;
        }

        internal void ResetTrigger(StateMachine stateMachine)
        {
            if(m_Type == StateMachineParameter.ParameterType.Trigger)
                stateMachine.ResetSetTrigger(_hashValue);
        }
    }
}
