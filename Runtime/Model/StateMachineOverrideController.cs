using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [System.Serializable]
    internal class StateActionPair
    {
        [SerializeField] internal string original;
        [SerializeField] internal string overridden;
    }

    public class StateMachineOverrideController : RuntimeStateMachineController
    {
        [SerializeField] private RuntimeStateMachineController m_Controller;
        [SerializeField] private List<StateActionPair> m_StateActionPairs;
        internal List<StateActionPair> stateActionPairs => m_StateActionPairs;
        public RuntimeStateMachineController runtimeStateMachineController { get => m_Controller; set => m_Controller = value; }

        internal int overridesCount
        {
            get
            {
                if (m_Controller == null)
                    return 0;
                return m_Controller.GetOriginalClipsCount;
            }
        }

        internal string GetOverrideAction(string originalAction)
        {
            var stateActionPair = m_StateActionPairs.Find(ele => ele.original.Equals(originalAction));
            return stateActionPair?.overridden;
        }
    }
}
