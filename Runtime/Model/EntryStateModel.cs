using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public sealed class EntryStateModel : TransitionNodeModel
    {
#if UNITY_EDITOR
        [SerializeField] private Vector3Int m_Position;
#endif
        [SerializeField] private StateTransitionModel[] m_Transitions = null;

        internal override ITransitionNode GetNode(
            StateMachine stateMachine,
            RuntimeStateGraph graph,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions)
        {
            if (cachedNodes.TryGetValue(this, out var node))
                return node;

            node = new EntryNode(stateMachine, graph, name);
            cachedNodes.Add(this, node);
            return node;
        }

        internal override void InitializeTransitions(
            StateMachine stateMachine,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions)
        {
            if (!cachedNodes.TryGetValue(this, out var node))
                return;

            node.TransitionStates = GetTransitions(stateMachine, cachedNodes, cachedActions, cachedConditions);
        }

        private TransitionState[] GetTransitions(
            StateMachine stateMachine,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions)
        {
            if (m_Transitions == null)
                return new TransitionState[0];

            var transitions = new TransitionState[m_Transitions.Length];
            for (int i = 0; i < m_Transitions.Length; ++i)
                transitions[i] = m_Transitions[i].GetTransition(stateMachine, cachedNodes, cachedActions, cachedConditions);

            return transitions;
        }
    }
}
