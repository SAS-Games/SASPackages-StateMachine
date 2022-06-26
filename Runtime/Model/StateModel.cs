using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
	public class StateModel : ScriptableObject
	{
#if UNITY_EDITOR
		[SerializeField] private Vector3 m_Position;
		public List<string> GetUsedTags()
		{
			List<string> tags = new List<string>();
			for (int i = 0; i < m_StateActions.Length; ++i)
			{
				if (!string.IsNullOrEmpty(m_StateActions[i].tag))
					tags.Add(m_StateActions[i].tag);
			}

			return tags;
		}

		public List<string> GetUsedKeys()
		{
			List<string> keys = new List<string>();
			for (int i = 0; i < m_StateActions.Length; ++i)
			{
				if (!string.IsNullOrEmpty(m_StateActions[i].key))
					keys.Add(m_StateActions[i].key);
			}

			return keys;
		}
#endif
		[SerializeField] private string m_Tag = "";
		[SerializeField] private StateActionModel[] m_StateActions = default;
		[SerializeField] private StateTransitionModel[] m_Transitions = null;

		public State State { get; private set; }


		internal State GetState(StateMachine stateMachine, Dictionary<ScriptableObject, object> cachedStates, Dictionary<StateActionModel, object[]> cachedActions, Dictionary<string, ICustomCondition> cachedConditions)
		{
			if (cachedStates.TryGetValue(this, out var obj))
				return (State)obj;

			var state = new State(stateMachine, name, m_Tag);
			State = state;
			cachedStates.Add(this, state);
			CreateGetActions(m_StateActions, stateMachine, state, cachedActions);
			state._transitionStates = GetTransitions(m_Transitions, stateMachine, cachedStates, cachedActions, cachedConditions);
			foreach(var transitionState in state._transitionStates)
				transitionState.StateEventForCustomTrigger(ref state._stateEnterEventForCustomeTriggers, ref state._stateExitEventForCustomeTriggers);
			return state;
		}

		private TransitionState[] GetTransitions(StateTransitionModel[] transitionModels, StateMachine stateMachine, Dictionary<ScriptableObject, object> cachedStates, Dictionary<StateActionModel, object[]> cachedActions, Dictionary<string, ICustomCondition> cachedConditions)
		{
			int count = transitionModels.Length;
			var transitions = new TransitionState[count];
			for (int i = 0; i < count; i++)
				transitions[i] = transitionModels[i].GetTransition(stateMachine, cachedStates, cachedActions, cachedConditions);

			return transitions;
		}
		
		private void CreateGetActions(StateActionModel[] scriptableActions, StateMachine stateMachine, State state, Dictionary<StateActionModel, object[]> createdInstances)
		{
			int count = scriptableActions.Length;
			var stateEnterActions = new List<IStateAction>();
			var stateFixedUpdateActions = new List<IStateAction>();
			var stateUpdateActions = new List<IStateAction>();
			var stateLateUpdateActions = new List<IStateAction>();
			var stateExitActions = new List<IStateAction>();

			for (int i = 0; i < count; i++)
			{
				var actions = scriptableActions[i].GetActions(stateMachine, state, createdInstances);

				int bits = (int)scriptableActions[i].whenToExecute;
				if (actions[0] is IAwaitableStateAction)
				{
					for (int k = 1; k <= 2; ++k)
					{
						if (IsKthBitSet(bits, k))
						{
							switch (k)
							{
								case 1:
									stateEnterActions.AddRange(actions);
									break;
								case 2:
									stateExitActions.AddRange(actions);
									break;
							}
						}
					}
				}
				else
				{
					for (int k = 1; k <= 5; ++k)
					{
						if (IsKthBitSet(bits, k))
						{
							switch (k)
							{
								case 1:
									stateEnterActions.AddRange(actions);
									break;
								case 2:
									stateFixedUpdateActions.AddRange(actions);
									break;
								case 3:
									stateUpdateActions.AddRange(actions);
									break;
								case 4:
									stateLateUpdateActions.AddRange(actions);
									break;
								case 5:
									stateExitActions.AddRange(actions);
									break;
							}
						}
					}
				}
			}

			state._onEnter = stateEnterActions.ToArray();
			state._onFixedUpdate = stateFixedUpdateActions.ToArray();
			state._onUpdate = stateUpdateActions.ToArray();
			state._onLateUpdate = stateLateUpdateActions.ToArray();
			state._onExit = stateExitActions.ToArray();
		}

		private bool IsKthBitSet(int n, int k)
		{
			return (n & (1 << (k - 1))) > 0;
		}
	}
}

