using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public class RuntimeStateMachineController : ScriptableObject
    {
        [SerializeField, HideInInspector] private StateMachineModel m_BaseStateMachineModel = default;
        [SerializeField, HideInInspector] private StateMachineParameter[] _parameters = default;
        [SerializeField, HideInInspector] private StateModel m_DefaultStateModel = default;
        [SerializeField, HideInInspector] private StateModel m_AnyStateModel = default;

        private void Awake()
        {
#if UNITY_EDITOR
            var controller = (this is StateMachineOverrideController)
                ? (this as StateMachineOverrideController).runtimeStateMachineController
                : this;
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            name = fileName;
#endif
        }

        internal StateMachine CreateStateMachine(Actor actor,
            StateMachineOverrideController stateMachineOverrideController)
        {
            var actionOverrides = new List<ActionOverride>();
            var stateOverrides = new List<StateOverride>();

            if (stateMachineOverrideController != null)
            {
                actionOverrides = stateMachineOverrideController.actionOverrides;
                stateOverrides = stateMachineOverrideController.stateOverrides;
            }

            StateMachine stateMachine = new StateMachine(actor, _parameters, stateOverrides, actionOverrides);
            var cachedNodes = new Dictionary<ScriptableObject, ITransitionNode>();
            var cachedActions = new Dictionary<StateActionModel, object[]>();
            var cachedTriggers = new Dictionary<string, ICustomCondition>();

            var rootGraph = m_BaseStateMachineModel.CreateRuntimeGraph(
                stateMachine,
                cachedNodes,
                cachedActions,
                cachedTriggers,
                m_DefaultStateModel);

            stateMachine.SetRootGraph(rootGraph);
            stateMachine.DefaultNode = rootGraph.DefaultNode;

            foreach (var pair in cachedNodes)
            {
                stateMachine.nodes.Add(pair.Value);
                if (pair.Value is State state)
                {
                    stateMachine.states.Add(state);
                    if (pair.Key == m_DefaultStateModel)
                        stateMachine.DefaultState = state;
                }
            }

            if (m_AnyStateModel != null)
            {
                var state = m_AnyStateModel.GetState(stateMachine, rootGraph, cachedNodes, cachedActions, cachedTriggers);
                m_AnyStateModel.InitializeTransitions(stateMachine, cachedNodes, cachedActions, cachedTriggers);
                if (!stateMachine.states.Contains(state))
                    stateMachine.states.Add(state);
                if (!stateMachine.nodes.Contains(state))
                    stateMachine.nodes.Add(state);
                stateMachine.AnyState = state;
            }

            if (stateMachine.DefaultState == null)
                stateMachine.DefaultState = stateMachine.DefaultNode?.ActiveState;

            return stateMachine;
        }

        internal void Initialize(RuntimeStateMachineController runtimeStateMachineController)
        {
            name = runtimeStateMachineController.name;
            _parameters = new StateMachineParameter[runtimeStateMachineController._parameters.Length];

            for (int i = 0; i < _parameters.Length; ++i)
                _parameters[i] = new StateMachineParameter(runtimeStateMachineController._parameters[i]);

            m_BaseStateMachineModel = runtimeStateMachineController.m_BaseStateMachineModel;
            m_DefaultStateModel = runtimeStateMachineController.m_DefaultStateModel;
            m_AnyStateModel = runtimeStateMachineController.m_AnyStateModel;
        }
    }
}
