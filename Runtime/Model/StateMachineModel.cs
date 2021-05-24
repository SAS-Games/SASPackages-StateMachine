using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public class StateMachineModel : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] private Vector3 m_Position = default;
        [SerializeField] private Vector3 m_AnyStatePosition = default;
        [SerializeField] private StateMachineModel m_ParentStateMachine = default;
#endif
        [SerializeField] private StateMachineModel[] m_ChildStateMachines = default;
        [SerializeField] private StateModel[] m_StateModels = default;

        private List<StateMachineModel> GetStateMachineRecursivily()
        {
            List<StateMachineModel> stateMachineModels = new List<StateMachineModel>();
            stateMachineModels.AddRange(m_ChildStateMachines);

            for (int i = 0; i < m_ChildStateMachines.Length; i++)
                stateMachineModels.AddRange(m_ChildStateMachines[i].GetStateMachineRecursivily());

            return stateMachineModels;
        }

        internal List<StateModel> GetStatesRecursivily()
        {
            var stateModels = new List<StateModel>();
            var childStateMachinesModel = new List<StateMachineModel>() { this };
            childStateMachinesModel.AddRange(GetStateMachineRecursivily());
            foreach (var csmm in childStateMachinesModel)
                stateModels.AddRange(csmm.m_StateModels);

            return stateModels;
        }
    }
}
