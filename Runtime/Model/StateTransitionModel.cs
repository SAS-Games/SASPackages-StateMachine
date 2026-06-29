using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [Serializable]
    public sealed class StateTransitionModel : ScriptableObject
    {
        [SerializeField] private TransitionNodeModel m_SourceNode;
        [SerializeField] private TransitionNodeModel m_TargetNode;
        [SerializeField] private StateModel m_SourceState;
        [SerializeField] private StateModel m_TargetState = default;
        [SerializeField] private bool m_HasExitTime = false;
        [SerializeField] private float m_ExitTime = 0;
        [SerializeField] private bool m_WaitForAwaitableActionsToComplete = true;
        [SerializeField] private Condition[] m_Conditions = default;

        public TransitionNodeModel SourceNodeModel => m_SourceNode != null ? m_SourceNode : m_SourceState;
        public TransitionNodeModel TargetNodeModel => m_TargetNode != null ? m_TargetNode : m_TargetState;

        internal TransitionState GetTransition(StateMachine stateMachine, Dictionary<ScriptableObject, ITransitionNode> cachedNodes, Dictionary<StateActionModel, object[]> cachedActions, Dictionary<string, ICustomCondition> cachedTriggers)
        {
            var targetModel = TargetNodeModel;
            if (targetModel == null)
            {
                Debug.LogError($"Transition {name} does not define a target node.");
                return new TransitionState(null, new Condition[0], m_HasExitTime, m_ExitTime, m_WaitForAwaitableActionsToComplete);
            }

            var sourceGraph = ResolveSourceGraph(cachedNodes);
            var node = targetModel.GetNode(stateMachine, ResolveGraph(targetModel, cachedNodes), cachedNodes, cachedActions, cachedTriggers);
            var entryOverrides = ResolveReachableTarget(sourceGraph, ref node);
            var conditions = GetConditions(stateMachine.Actor, cachedTriggers);
            return new TransitionState(node, conditions, m_HasExitTime, m_ExitTime, m_WaitForAwaitableActionsToComplete, entryOverrides);
        }

        private RuntimeStateGraph ResolveSourceGraph(Dictionary<ScriptableObject, ITransitionNode> cachedNodes)
        {
            return SourceNodeModel != null && cachedNodes.TryGetValue(SourceNodeModel, out var sourceNode)
                ? sourceNode.Graph
                : null;
        }

        private RuntimeStateGraph ResolveGraph(TransitionNodeModel targetModel, Dictionary<ScriptableObject, ITransitionNode> cachedNodes)
        {
            if (targetModel != null && cachedNodes.TryGetValue(targetModel, out var cachedNode))
                return cachedNode.Graph;

            return SourceNodeModel != null && cachedNodes.TryGetValue(SourceNodeModel, out var sourceNode)
                ? sourceNode.Graph
                : null;
        }

        private TransitionEntryOverride[] ResolveReachableTarget(RuntimeStateGraph sourceGraph, ref ITransitionNode targetNode)
        {
            if (sourceGraph == null || targetNode == null || targetNode.Graph == sourceGraph)
                return null;

            var entryOverrides = new List<TransitionEntryOverride>();
            var entryTarget = targetNode;

            var graph = targetNode.Graph;
            while (graph != null && graph.ParentGraph != sourceGraph)
            {
                if (graph.OwnerSubStateMachine != null)
                {
                    entryOverrides.Add(new TransitionEntryOverride(graph.OwnerSubStateMachine, entryTarget));
                    entryTarget = graph.OwnerSubStateMachine;
                }

                graph = graph.ParentGraph;
            }

            if (graph?.OwnerSubStateMachine == null)
                return entryOverrides.Count > 0 ? entryOverrides.ToArray() : null;

            entryOverrides.Add(new TransitionEntryOverride(graph.OwnerSubStateMachine, entryTarget));
            targetNode = graph.OwnerSubStateMachine;

            return entryOverrides.ToArray();
        }

        private Condition[] GetConditions(Actor actor, Dictionary<string, ICustomCondition> cachedCustomConditions)
        {
            if (m_Conditions == null)
                return new Condition[0];

            var result = new Condition[m_Conditions.Length];
            for (int i = 0; i < m_Conditions.Length; ++i)
            {
                result[i] = m_Conditions[i].Clone();
                if (result[i].m_Type == StateMachineParameter.ParameterType.Custom)
                    result[i].Custom = CustomTrigger(actor, m_Conditions[i].m_CustomCondition, cachedCustomConditions);
            }
            return result;
        }

        private ICustomCondition CustomTrigger(Actor actor, string typeName, Dictionary<string, ICustomCondition> cachedCustomConditions)
        {
            if (!string.IsNullOrEmpty(typeName))
            {
                if (!cachedCustomConditions.TryGetValue(typeName, out var trigger))
                {
                    trigger = (ICustomCondition)Activator.CreateInstance(Type.GetType(typeName));
                    cachedCustomConditions[typeName] = trigger;
                    trigger.OnInitialize(actor);
                }
                return trigger;
            }
            return null;
        }
    }
}
