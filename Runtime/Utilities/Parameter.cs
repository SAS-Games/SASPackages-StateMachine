using System;
using UnityEngine;
using static SAS.StateMachineGraph.StateMachineParameter;

namespace SAS.StateMachineGraph.Utilities
{
	[System.Serializable]
	public struct Parameter
	{
		[SerializeField] private string m_Name;
		[SerializeField] private ParameterType m_Type;
		[SerializeField] private bool m_BoolValue;
		[SerializeField] private int m_IntValue;
		[SerializeField] private float m_FloatValue;

		internal string Name => m_Name;
		internal ParameterType Type => m_Type;
		internal bool BoolValue => m_BoolValue;
		internal int IntValue => m_IntValue;
		internal float FloatValue => m_FloatValue;

	}

	[CreateAssetMenu(menuName = "SAS/State Machine Controller/Parameters")]
	public class ParameterConfig : ScriptableObject
	{
		[SerializeField] private Parameter[] m_Parameters;

		internal bool TryGet(string key, out Parameter parameter)
		{
			int index = -1;
			index = Array.FindIndex(m_Parameters, ele => ele.Name.Equals(key));
			if (index != -1)
				parameter = m_Parameters[index];
			else 
				parameter = default;

			return index != -1;
		}
	}
}
