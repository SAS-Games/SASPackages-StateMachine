using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
	public class StateModel : ScriptableObject
	{
#if UNITY_EDITOR
		[SerializeField] private Vector3Int m_Position;
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
			StateModel originalModel = this;

			// Resolve override data source (read-only)
			StateModel overrideModel = null;
			var stateOverride = stateMachine.stateOverrides?.Find(p => p.original == this);
			if (stateOverride != null)
				overrideModel = stateOverride.overridden;

			// Cache by ORIGINAL model identity
			if (cachedStates.TryGetValue(originalModel, out var obj))
				return (State)obj;

			// Choose data sources
			var actionsSource = overrideModel != null ? overrideModel.m_StateActions : originalModel.m_StateActions;
			var tagSource = overrideModel != null ? overrideModel.m_Tag : originalModel.m_Tag;
			var nameSource = originalModel.name;

			// Build state
			var state = new State(stateMachine, nameSource, tagSource);
			originalModel.State = state;
			cachedStates.Add(originalModel, state);

			// Actions from selected source
			CreateGetActions(actionsSource, stateMachine, state, cachedActions);

			// Transitions ALWAYS from original
			state._transitionStates = originalModel.GetTransitions(originalModel.m_Transitions, stateMachine, cachedStates, cachedActions, cachedConditions);

			foreach (var transitionState in state._transitionStates)
			{
				transitionState.StateEventForCustomTrigger(
					ref state._stateEnterEventForCustomTriggers,
					ref state._stateExitEventForCustomTriggers
				);
			}

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
				var actions = scriptableActions[i].GetActions(stateMachine, createdInstances);

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

