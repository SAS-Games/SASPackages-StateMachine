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
        [SerializeField] private StateMachineParameter.ParameterType m_Type;
        [SerializeField] private float m_FloatValue;
        [SerializeField] internal string m_CustomTrigger;

        internal ICustomTrigger CustomTrigger;

        internal bool IsValid(StateMachine stateMachine)
        {
            switch (m_Type)
            {
                case StateMachineParameter.ParameterType.Bool:
                    return stateMachine.GetBool(m_Name) == (m_Mode == Mode.If);

                case StateMachineParameter.ParameterType.Int:
                    if (m_Mode == Mode.Greater)
                        return stateMachine.GetInteger(m_Name) > (int)m_FloatValue;
                    else if (m_Mode == Mode.Less)
                        return stateMachine.GetInteger(m_Name) < (int)m_FloatValue;
                    if (m_Mode == Mode.Equals)
                        return stateMachine.GetInteger(m_Name) == (int)m_FloatValue;
                    return stateMachine.GetInteger(m_Name) != (int)m_FloatValue;

                case StateMachineParameter.ParameterType.Float:
                    if (m_Mode == Mode.Greater)
                        return stateMachine.GetFloat(m_Name) > m_FloatValue;
                    return stateMachine.GetFloat(m_Name) < m_FloatValue;

                case StateMachineParameter.ParameterType.Trigger:
                    var resut = stateMachine.GetBool(m_Name) == true;
                    stateMachine.ResetSetTrigger(m_Name);
                    return resut;
                case StateMachineParameter.ParameterType.CustomTrigger:
                    return CustomTrigger.Evaluate();
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
            clone.m_CustomTrigger = null;
            return clone;
        }
    }
}
