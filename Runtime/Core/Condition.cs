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

#if UNITY_EDITOR
        internal StateMachineDebugConditionResult CreateDebugConditionResult(int index)
        {
            return new StateMachineDebugConditionResult(
                index,
                GetDebugName(),
                m_Type.ToString(),
                m_Mode.ToString(),
                string.Empty,
                GetDebugExpectedValue(),
                false,
                false,
                string.Empty,
                Custom);
        }

        internal StateMachineDebugConditionResult EvaluateDebug(StateMachine stateMachine, int index)
        {
            var actualValue = string.Empty;
            var result = false;

            switch (m_Type)
            {
                case StateMachineParameter.ParameterType.Bool:
                {
                    var value = stateMachine.GetBool(_hashValue);
                    actualValue = value.ToString();
                    result = value == (m_Mode == Mode.If);
                    break;
                }
                case StateMachineParameter.ParameterType.Int:
                {
                    var value = stateMachine.GetInteger(_hashValue);
                    actualValue = value.ToString();
                    if (m_Mode == Mode.Greater)
                        result = value > (int)m_FloatValue;
                    else if (m_Mode == Mode.Less)
                        result = value < (int)m_FloatValue;
                    else if (m_Mode == Mode.Equals)
                        result = value == (int)m_FloatValue;
                    else
                        result = value != (int)m_FloatValue;
                    break;
                }
                case StateMachineParameter.ParameterType.Float:
                {
                    var value = stateMachine.GetFloat(_hashValue);
                    actualValue = value.ToString("F3");
                    result = m_Mode == Mode.Greater ? value > m_FloatValue : value < m_FloatValue;
                    break;
                }
                case StateMachineParameter.ParameterType.Trigger:
                {
                    var value = stateMachine.GetBool(_hashValue);
                    actualValue = value.ToString();
                    result = value;
                    break;
                }
                case StateMachineParameter.ParameterType.Custom:
                {
                    var value = Custom.Evaluate();
                    actualValue = value.ToString();
                    result = value == (m_Mode == Mode.If);
                    break;
                }
            }

            return new StateMachineDebugConditionResult(
                index,
                GetDebugName(),
                m_Type.ToString(),
                m_Mode.ToString(),
                actualValue,
                GetDebugExpectedValue(),
                true,
                result,
                string.Empty,
                Custom);
        }

        private string GetDebugName()
        {
            if (!string.IsNullOrEmpty(m_Name))
                return m_Name;

            if (Custom != null)
                return Custom.GetType().Name;

            if (!string.IsNullOrEmpty(m_CustomCondition))
                return m_CustomCondition;

            return "<Unnamed>";
        }

        private string GetDebugExpectedValue()
        {
            switch (m_Type)
            {
                case StateMachineParameter.ParameterType.Bool:
                    return (m_Mode == Mode.If).ToString();
                case StateMachineParameter.ParameterType.Int:
                    return $"{m_Mode} {(int)m_FloatValue}";
                case StateMachineParameter.ParameterType.Float:
                    return $"{m_Mode} {m_FloatValue:F3}";
                case StateMachineParameter.ParameterType.Trigger:
                    return "True";
                case StateMachineParameter.ParameterType.Custom:
                    return (m_Mode == Mode.If).ToString();
                default:
                    return string.Empty;
            }
        }
#endif

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
