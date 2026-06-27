using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public class StateMachineModel : TransitionNodeModel
    {
#if UNITY_EDITOR
        [SerializeField] private Vector3Int m_Position = new Vector3Int(300, 50, 0);
        [SerializeField] private Vector3Int m_PositionAsUpNode = new Vector3Int(700, 100);
        [SerializeField] private Vector3Int m_AnyStatePosition;
        [SerializeField] private StateMachineModel m_ParentStateMachine;
#endif
        [SerializeField] private StateMachineModel[] m_ChildStateMachines = default;
        [SerializeField] private StateModel[] m_StateModels = default;
        [SerializeField] private EntryStateModel m_EntryNode = default;
        [SerializeField] private ExitStateModel m_ExitNode = default;
        [SerializeField] private StateTransitionModel[] m_Transitions = null;

        internal EntryStateModel EntryNodeModel => m_EntryNode;
        internal ExitStateModel ExitNodeModel => m_ExitNode;

        internal RuntimeStateGraph CreateRuntimeGraph(
            StateMachine stateMachine,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions,
            TransitionNodeModel defaultNodeModel)
        {
            var graph = new RuntimeStateGraph(stateMachine, name);
            BuildRuntimeGraph(stateMachine, graph, cachedNodes, cachedActions, cachedConditions, defaultNodeModel);
            return graph;
        }

        internal override ITransitionNode GetNode(
            StateMachine stateMachine,
            RuntimeStateGraph graph,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions)
        {
            if (cachedNodes.TryGetValue(this, out var node))
                return node;

            var childGraph = new RuntimeStateGraph(stateMachine, name);
            var subStateMachine = new SubStateMachine(stateMachine, graph, childGraph, name);
            cachedNodes.Add(this, subStateMachine);
            BuildRuntimeGraph(stateMachine, childGraph, cachedNodes, cachedActions, cachedConditions, null);
            return subStateMachine;
        }

        internal override void InitializeTransitions(
            StateMachine stateMachine,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions)
        {
            if (!cachedNodes.TryGetValue(this, out var node))
                return;

            node.TransitionStates = GetTransitions(m_Transitions, stateMachine, cachedNodes, cachedActions, cachedConditions);
        }

        private void BuildRuntimeGraph(
            StateMachine stateMachine,
            RuntimeStateGraph graph,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions,
            TransitionNodeModel defaultNodeModel)
        {
            graph.EntryNode = m_EntryNode != null
                ? m_EntryNode.GetNode(stateMachine, graph, cachedNodes, cachedActions, cachedConditions)
                : new EntryNode(stateMachine, graph, $"{name} Entry");

            graph.ExitNode = m_ExitNode != null
                ? m_ExitNode.GetNode(stateMachine, graph, cachedNodes, cachedActions, cachedConditions)
                : new ExitNode(graph, $"{name} Exit");

            var localNodeModels = GetLocalNodeModels();
            for (int i = 0; i < localNodeModels.Count; ++i)
                localNodeModels[i].GetNode(stateMachine, graph, cachedNodes, cachedActions, cachedConditions);

            for (int i = 0; i < localNodeModels.Count; ++i)
                localNodeModels[i].InitializeTransitions(stateMachine, cachedNodes, cachedActions, cachedConditions);

            m_EntryNode?.InitializeTransitions(stateMachine, cachedNodes, cachedActions, cachedConditions);
            m_ExitNode?.InitializeTransitions(stateMachine, cachedNodes, cachedActions, cachedConditions);

            graph.DefaultNode = ResolveDefaultNode(graph, defaultNodeModel, localNodeModels, cachedNodes);
            if (graph.EntryNode is EntryNode entryNode)
                entryNode.FallbackTarget = graph.DefaultNode;
        }

        private List<TransitionNodeModel> GetLocalNodeModels()
        {
            var nodeModels = new List<TransitionNodeModel>();
            if (m_StateModels != null)
            {
                for (int i = 0; i < m_StateModels.Length; ++i)
                {
                    if (m_StateModels[i] != null)
                        nodeModels.Add(m_StateModels[i]);
                }
            }

            if (m_ChildStateMachines != null)
            {
                for (int i = 0; i < m_ChildStateMachines.Length; ++i)
                {
                    if (m_ChildStateMachines[i] != null)
                        nodeModels.Add(m_ChildStateMachines[i]);
                }
            }

            return nodeModels;
        }

        private ITransitionNode ResolveDefaultNode(
            RuntimeStateGraph graph,
            TransitionNodeModel defaultNodeModel,
            List<TransitionNodeModel> localNodeModels,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes)
        {
            if (defaultNodeModel != null &&
                cachedNodes.TryGetValue(defaultNodeModel, out var defaultNode) &&
                defaultNode.Graph == graph)
                return defaultNode;

            for (int i = 0; i < localNodeModels.Count; ++i)
            {
                if (cachedNodes.TryGetValue(localNodeModels[i], out var node))
                    return node;
            }

            return null;
        }

        private TransitionState[] GetTransitions(
            StateTransitionModel[] transitionModels,
            StateMachine stateMachine,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions)
        {
            if (transitionModels == null)
                return new TransitionState[0];

            var transitions = new TransitionState[transitionModels.Length];
            for (int i = 0; i < transitionModels.Length; ++i)
                transitions[i] = transitionModels[i].GetTransition(stateMachine, cachedNodes, cachedActions, cachedConditions);

            return transitions;
        }

        private List<StateMachineModel> GetStateMachineRecursivily()
        {
            List<StateMachineModel> stateMachineModels = new List<StateMachineModel>();
            if (m_ChildStateMachines == null)
                return stateMachineModels;

            stateMachineModels.AddRange(m_ChildStateMachines);

            for (int i = 0; i < m_ChildStateMachines.Length; i++)
            {
                if (m_ChildStateMachines[i] != null)
                    stateMachineModels.AddRange(m_ChildStateMachines[i].GetStateMachineRecursivily());
            }

            return stateMachineModels;
        }

        internal List<StateModel> GetStatesRecursivily()
        {
            var stateModels = new List<StateModel>();
            var childStateMachinesModel = new List<StateMachineModel>() { this };
            childStateMachinesModel.AddRange(GetStateMachineRecursivily());
            foreach (var csmm in childStateMachinesModel)
            {
                if (csmm?.m_StateModels != null)
                    stateModels.AddRange(csmm.m_StateModels);
            }

            return stateModels;
        }
    }
}
