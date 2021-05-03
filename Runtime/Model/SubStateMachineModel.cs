using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public class SubStateMachineModel : Object
    {
        [SerializeField] private Vector3 m_Position;
        [SerializeField] private StateModel[] m_stateModels;
    }
}
