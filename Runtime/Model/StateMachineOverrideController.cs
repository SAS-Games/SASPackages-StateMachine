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

    [CreateAssetMenu(menuName = "StateMachineOverrideController")]
    public class StateMachineOverrideController : RuntimeStateMachineController
    {
        [SerializeField] private RuntimeStateMachineController m_Controller;
        [SerializeField] private List<StateActionPair> m_StateActionPairs;

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

        /*  public void ApplyOverrides(IList<KeyValuePair<StateActionModel, StateActionModel>> overrides)
          {
              if (overrides == null)
                  throw new System.ArgumentNullException("overrides");

                for (int i = 0; i < overrides.Count; i++)
                    SetClip(overrides[i].Key, overrides[i].Value, false);

                SendNotification();
          }*/

        internal string GetOverrideAction(string originalAction)
        {
            var stateActionPair = m_StateActionPairs.Find(ele => ele.original.Equals(originalAction));
            return stateActionPair?.overridden;
        }
    }
}
