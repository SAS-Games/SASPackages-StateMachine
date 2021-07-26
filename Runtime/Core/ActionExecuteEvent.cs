using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [System.Flags]
    public enum ActionExecuteEvent
    {
        OnStateEnter = 1 << 0,
        OnFixedUpdate = 1 << 1,
        OnUpdate = 1 << 2,
        OnLateUpdate = 1 << 3,
        OnStateExit = 1 << 4
    }
}
