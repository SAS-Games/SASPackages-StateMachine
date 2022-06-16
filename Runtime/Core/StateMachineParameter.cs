using System;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [Serializable]
    internal class StateMachineParameter
    {
        public enum ParameterType
        {
            Float = 1,
            Int = 3,
            Bool = 4,
            Trigger = 9,
            CustomTrigger = 0
        }

        [SerializeField] private string m_Name;
        [SerializeField] private ParameterType m_Type;
        [SerializeField] private float m_DefaultFloat;
        [SerializeField] private int m_DefaultInt;
        [SerializeField] private bool m_DefaultBool;

        internal StateMachineParameter(StateMachineParameter parameter)
        {
            m_Name = parameter.m_Name;
            m_Type = parameter.m_Type;
            m_DefaultFloat = parameter.m_DefaultFloat;
            m_DefaultInt = parameter.m_DefaultInt;
            m_DefaultBool = parameter.m_DefaultBool;
        }

        public string name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        internal ParameterType Type { get { return m_Type; } set { m_Type = value; } }
        internal float FloatValue { get { return m_DefaultFloat; } set { m_DefaultFloat = value; } }
        internal int IntValue { get { return m_DefaultInt; } set { m_DefaultInt = value; } }
        internal bool BoolValue { get { return m_DefaultBool; } set { m_DefaultBool = value; } }

        public override bool Equals(object o)
        {
            StateMachineParameter other = (StateMachineParameter)o;
            return m_Name == other.m_Name && m_Type == other.m_Type && m_DefaultFloat == other.m_DefaultFloat && m_DefaultInt == other.m_DefaultInt && m_DefaultBool == other.m_DefaultBool;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}