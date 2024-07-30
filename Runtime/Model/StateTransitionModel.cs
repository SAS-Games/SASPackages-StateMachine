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

        internal TransitionState GetTransition(StateMachine stateMachine, Dictionary<ScriptableObject, object> cachedStates, Dictionary<StateActionModel, object[]> cachedActions, Dictionary<string, ICustomCondition> cachedTriggers)
        {
            var state = m_TargetState.GetState(stateMachine, cachedStates, cachedActions, cachedTriggers);
            var conditions = GetConditions(stateMachine.Actor, cachedTriggers);
            return new TransitionState(state, conditions, m_HasExitTime, m_ExitTime, m_WaitForAwaitableActionsToComplete);
        }

        private Condition[] GetConditions(Actor actor, Dictionary<string, ICustomCondition> cachedCustomConditions)
        {
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
