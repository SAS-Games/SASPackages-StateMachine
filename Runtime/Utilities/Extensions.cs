using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
#pragma warning disable 0649
    [System.Serializable]
    public struct Parameter
    {
        [SerializeField] private string m_Name;
        [SerializeField] private ParameterType m_Type;
        [SerializeField] private bool m_BoolValue;
        [SerializeField] private int m_IntValue;
        [SerializeField] private float m_FloatValue;

        public string Name => m_Name;
        public ParameterType Type => m_Type;
        public bool BoolValue => m_BoolValue;
        public int IntValue => m_IntValue;
        public float FloatValue => m_FloatValue;

    }
#pragma warning restore 0649
    public enum ParameterType
    {
        Float = 1,
        Int = 3,
        Bool = 4,
        Trigger = 9
    }

    public static class Extensions
    {
        public static void Apply(this Animator animator, in Parameter parameter)
        {
            switch (parameter.Type)
            {
                case ParameterType.Bool:
                    animator.SetBool(parameter.Name, parameter.BoolValue);
                    break;
                case ParameterType.Int:
                    animator.SetInteger(parameter.Name, parameter.IntValue);
                    break;
                case ParameterType.Float:
                    animator.SetFloat(parameter.Name, parameter.FloatValue);
                    break;
                case ParameterType.Trigger:
                    animator.SetTrigger(parameter.Name);
                    break;
            }
        }
    }
}
