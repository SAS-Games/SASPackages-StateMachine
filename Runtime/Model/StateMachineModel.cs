using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public class StateMachineModel : ScriptableObject
    {
        [SerializeField] private Vector3 m_Position;
        [SerializeField] private Vector3 m_AnyStatePosition;
        [SerializeField] private StateMachineModel m_ParentStateMachine;
        [SerializeField] private StateMachineModel[] m_ChildStateMachines;
        [SerializeField] private StateModel[] m_StateModels;
    }
}
