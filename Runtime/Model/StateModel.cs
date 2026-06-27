using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
	public class StateModel : TransitionNodeModel
	{
#if UNITY_EDITOR
		[SerializeField] private Vector3Int m_Position;
		public List<string> GetUsedKeys()
		{
			List<string> keys = new List<string>();
			if (m_StateActions == null)
				return keys;

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


		internal override ITransitionNode GetNode(StateMachine stateMachine, RuntimeStateGraph graph, Dictionary<ScriptableObject, ITransitionNode> cachedNodes, Dictionary<StateActionModel, object[]> cachedActions, Dictionary<string, ICustomCondition> cachedConditions)
		{
			return GetState(stateMachine, graph, cachedNodes, cachedActions, cachedConditions);
		}

		internal State GetState(StateMachine stateMachine, RuntimeStateGraph graph, Dictionary<ScriptableObject, ITransitionNode> cachedNodes, Dictionary<StateActionModel, object[]> cachedActions, Dictionary<string, ICustomCondition> cachedConditions)
		{
			StateModel originalModel = this;

			// Resolve override data source (read-only)
			StateModel overrideModel = null;
			var stateOverride = stateMachine.stateOverrides?.Find(p => p.original == this);
			if (stateOverride != null)
				overrideModel = stateOverride.overridden;

			// Cache by ORIGINAL model identity
			if (cachedNodes.TryGetValue(originalModel, out var obj))
				return (State)obj;

			// Choose data sources
			var actionsSource = overrideModel != null ? overrideModel.m_StateActions : originalModel.m_StateActions;
			var tagSource = overrideModel != null ? overrideModel.m_Tag : originalModel.m_Tag;
			var nameSource = originalModel.name;

			// Build state
			var state = new State(stateMachine, graph, nameSource, tagSource);
			originalModel.State = state;
			cachedNodes.Add(originalModel, state);

			// Actions from selected source
			CreateGetActions(actionsSource, stateMachine, state, cachedActions);

			return state;
		}

		internal override void InitializeTransitions(StateMachine stateMachine, Dictionary<ScriptableObject, ITransitionNode> cachedNodes, Dictionary<StateActionModel, object[]> cachedActions, Dictionary<string, ICustomCondition> cachedConditions)
		{
			if (!cachedNodes.TryGetValue(this, out var node))
				return;

			var state = (State)node;
			state._stateEnterEventForCustomTriggers.Clear();
			state._stateExitEventForCustomTriggers.Clear();
			state._transitionStates = GetTransitions(m_Transitions, stateMachine, cachedNodes, cachedActions, cachedConditions);
			foreach (var transitionState in state._transitionStates)
			{
				transitionState.StateEventForCustomTrigger(
					ref state._stateEnterEventForCustomTriggers,
					ref state._stateExitEventForCustomTriggers
				);
			}
		}


		private TransitionState[] GetTransitions(StateTransitionModel[] transitionModels, StateMachine stateMachine, Dictionary<ScriptableObject, ITransitionNode> cachedNodes, Dictionary<StateActionModel, object[]> cachedActions, Dictionary<string, ICustomCondition> cachedConditions)
		{
			if (transitionModels == null)
				return new TransitionState[0];

			int count = transitionModels.Length;
			var transitions = new TransitionState[count];
			for (int i = 0; i < count; i++)
				transitions[i] = transitionModels[i].GetTransition(stateMachine, cachedNodes, cachedActions, cachedConditions);

			return transitions;
		}
		
		private void CreateGetActions(StateActionModel[] scriptableActions, StateMachine stateMachine, State state, Dictionary<StateActionModel, object[]> createdInstances)
		{
			if (scriptableActions == null)
				scriptableActions = new StateActionModel[0];

			int count = scriptableActions.Length;
			var stateEnterActions = new List<IStateAction>();
			var stateFixedUpdateActions = new List<IStateAction>();
			var stateUpdateActions = new List<IStateAction>();
			var stateLateUpdateActions = new List<IStateAction>();
			var stateExitActions = new List<IStateAction>();

			for (int i = 0; i < count; i++)
			{
				var actions = scriptableActions[i].GetActions(stateMachine, createdInstances);
				if (actions == null || actions.Length == 0)
					continue;

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

