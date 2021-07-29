using System;
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

        public static T[] Add<T>(this T[] array, T item)
        {
            if (array == null)
            {
                return new T[] { item };
            }
            Array.Resize(ref array, array.Length + 1);
            array[array.Length - 1] = item;

            return array;
        }

        public static T[] AddRange<T>(this T[] array, params T[] items)
        {
            if (array == null)
                array = new T[] { };

            if (items == null)
                return array;

            Array.Resize(ref array, array.Length + items.Length);
            Array.Copy(items, 0, array, array.Length - items.Length, items.Length);
            return array;
        }
    }
}
