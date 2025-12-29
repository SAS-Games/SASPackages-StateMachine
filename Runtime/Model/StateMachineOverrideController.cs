using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [System.Serializable]
    internal class ActionOverride
    {
        [SerializeField] internal string original;
        [SerializeField] internal string overridden;
    }

    [System.Serializable]
    internal class StateOverride
    {
        [SerializeField] internal StateModel original;
        [SerializeField] internal StateModel overridden;
    }

    public class StateMachineOverrideController : RuntimeStateMachineController
    {
        [SerializeField] private RuntimeStateMachineController m_Controller;

        [SerializeField] private List<ActionOverride> m_ActionOverrides;
        [SerializeField] private List<StateOverride> m_StateOverrides;

        internal List<ActionOverride> actionOverrides => m_ActionOverrides;
        internal List<StateOverride> stateOverrides => m_StateOverrides;

        public RuntimeStateMachineController runtimeStateMachineController
        {
            get => m_Controller;
            set => m_Controller = value;
        }

        internal string GetOverrideAction(string originalAction)
        {
            if (string.IsNullOrEmpty(originalAction) || m_ActionOverrides == null)
                return null;

            var pair = m_ActionOverrides.Find(p => p.original == originalAction);
            return pair?.overridden;
        }

        public bool TryGetOverrideState(StateModel originalState, out StateModel overridden)
        {
            overridden = null;
            if (originalState == null || m_StateOverrides == null)
                return false;

            var pair = m_StateOverrides.Find(p => p.original == originalState);
            overridden = pair?.overridden;
            return overridden != null;
        }
    }
}