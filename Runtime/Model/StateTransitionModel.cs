using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
	[Serializable]
	internal class StateTransitionModel
	{
		[SerializeField] private StateModel m_TargetState = default;
		[SerializeField] private Condition[] m_Conditions = default;

        public StateModel TargetState { get => m_TargetState; }

        internal TransitionState GetTransition(StateMachine stateMachine, Dictionary<ScriptableObject, object> cachedStates, Dictionary<StateActionModel, object[]> cachedActions)
		{
			var state = m_TargetState.GetState(stateMachine, cachedStates, cachedActions);
			var conditions = GetConditions();
			return new TransitionState(state, conditions);
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
