using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
	[Serializable]
	public sealed class StateTransitionModel : ScriptableObject
	{
		[SerializeField] private StateModel m_SourceState = default;
		[SerializeField] private StateModel m_TargetState = default;
		[SerializeField] private bool m_HasExitTime = false;
		[SerializeField] private float m_ExitTime = 0;
		[SerializeField] private bool m_WaitForAwaitableActionsToComplete = true;
		[SerializeField] private Condition[] m_Conditions = default;

		internal TransitionState GetTransition(StateMachine stateMachine, Dictionary<ScriptableObject, object> cachedStates, Dictionary<StateActionModel, object[]> cachedActions, Dictionary<string, ICustomTrigger> cachedTriggers)
		{
			var state = m_TargetState.GetState(stateMachine, cachedStates, cachedActions, cachedTriggers);
			var conditions = GetConditions(cachedTriggers);
			return new TransitionState(state, conditions, m_HasExitTime, m_ExitTime, m_WaitForAwaitableActionsToComplete);
		}

		private Condition[] GetConditions(Dictionary<string, ICustomTrigger> cachedTriggers)
		{
			var result = new Condition[m_Conditions.Length];
			for (int i = 0; i < m_Conditions.Length; ++i)
			{
				result[i] = m_Conditions[i].Clone();
				result[i].CustomTrigger = CustomTrigger(m_Conditions[i].m_CustomTrigger, cachedTriggers);
			}
			return result;
		}

		private ICustomTrigger CustomTrigger(string typeName, Dictionary<string, ICustomTrigger> cachedTriggers)
		{
			if (!string.IsNullOrEmpty(typeName))
			{
				if (!cachedTriggers.TryGetValue(typeName, out var trigger))
				{
					trigger = (ICustomTrigger)Activator.CreateInstance(Type.GetType(typeName));
					cachedTriggers[typeName] = trigger;
				}
				return trigger;
			}
			return null;
		}
	}
}
