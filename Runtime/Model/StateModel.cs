using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{ 
	public class StateModel : ScriptableObject
	{
		[SerializeField] private Vector3 position;
		[SerializeField] private StateActionModel[] m_StateEnterActions = default;
		[SerializeField] private StateActionModel[] m_StateUpdateActions = default;
		[SerializeField] private StateActionModel[] m_StateFixedUpdateActions = default;
		[SerializeField] private StateActionModel[] m_StateExitActions = default;



		[SerializeField] private StateTransitionModel[] m_Transitions = null;

		public override bool Equals(object other)
		{
			return name.Equals((other as StateModel).name);
		}

		public override int GetHashCode()
		{
			return name.GetHashCode();
		}

		internal State GetState(StateMachine stateMachine, Dictionary<ScriptableObject, object> cachedStates, Dictionary<StateActionModel, object> cachedActions)
		{
			if (cachedStates.TryGetValue(this, out var obj))
				return (State)obj;

			var state = new State(stateMachine, name);
			cachedStates.Add(this, state);

			state._onEnter = GetActions(m_StateEnterActions, stateMachine, cachedActions);
			state._onExit = GetActions(m_StateExitActions, stateMachine, cachedActions);
			state._onUpdate = GetActions(m_StateUpdateActions, stateMachine, cachedActions);
			state._onFixedUpdate = GetActions(m_StateFixedUpdateActions, stateMachine, cachedActions);
			state._transitionStates = GetTransitions(m_Transitions, stateMachine, cachedStates, cachedActions);

			return state;
		}

		private TransitionState[] GetTransitions(StateTransitionModel[] transitionModels, StateMachine stateMachine, Dictionary<ScriptableObject, object> cachedStates, Dictionary<StateActionModel, object> cachedActions)
		{
			int count = transitionModels.Length;
			var transitions = new TransitionState[count];
			for (int i = 0; i < count; i++)
				transitions[i] = transitionModels[i].GetTransition(stateMachine, cachedStates, cachedActions);

			return transitions;
		}

		private IStateAction[] GetActions(StateActionModel[] scriptableActions, StateMachine stateMachine, Dictionary<StateActionModel, object> createdInstances)
		{
			int count = scriptableActions.Length;
			var actions = new IStateAction[count];
			for (int i = 0; i < count; i++)
			{
				var action = scriptableActions[i].GetAction(stateMachine, createdInstances);
				if (action != null)
					actions[i] = action;
			}

			return actions;
		}

		public int GetTransitionStateIndex(StateModel state)
		{
			return Array.IndexOf(m_Transitions, (Array.Find(m_Transitions, ele => ele.TargetState == state)));
		}
	}
}

