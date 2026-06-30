using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public sealed class ExitStateModel : TransitionNodeModel
    {
#if UNITY_EDITOR
        [SerializeField] private Vector3Int m_Position;
#endif
#pragma warning disable 0414
        [SerializeField] private StateTransitionModel[] m_Transitions = null;
#pragma warning restore 0414

        internal override ITransitionNode GetNode(
            StateMachine stateMachine,
            RuntimeStateGraph graph,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions)
        {
            if (cachedNodes.TryGetValue(this, out var node))
                return node;

            node = new ExitNode(graph, name);
            cachedNodes.Add(this, node);
            return node;
        }

        internal override void InitializeTransitions(
            StateMachine stateMachine,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions)
        {
            if (cachedNodes.TryGetValue(this, out var node))
                node.TransitionStates = new TransitionState[0];
        }
    }
}
