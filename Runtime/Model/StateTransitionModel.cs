using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
	[Serializable]
	internal class StateTransitionModel
	{
		[SerializeField] private StateModel m_TargetState = default;
		[SerializeField] private bool m_HasExitTime = false;
		[SerializeField] private float m_ExitTime = 0;
		[SerializeField] private Condition[] m_Conditions = default;

        public StateModel TargetState { get => m_TargetState; }

        internal TransitionState GetTransition(StateMachine stateMachine, Dictionary<ScriptableObject, object> cachedStates, Dictionary<StateActionModel, object[]> cachedActions)
		{
			var state = m_TargetState.GetState(stateMachine, cachedStates, cachedActions);
			var conditions = GetConditions();
			return new TransitionState(state, conditions, m_HasExitTime, m_ExitTime);
		}

		private Condition[] GetConditions()
		{
			var result = new Condition[m_Conditions.Length];
			for (int i = 0; i < m_Conditions.Length; ++i)
				result[i] = m_Conditions[i].Clone();
			return result;
		}
	}
}
