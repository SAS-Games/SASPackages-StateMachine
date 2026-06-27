using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public abstract class TransitionNodeModel : ScriptableObject
    {
        internal abstract ITransitionNode GetNode(
            StateMachine stateMachine,
            RuntimeStateGraph graph,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions);

        internal abstract void InitializeTransitions(
            StateMachine stateMachine,
            Dictionary<ScriptableObject, ITransitionNode> cachedNodes,
            Dictionary<StateActionModel, object[]> cachedActions,
            Dictionary<string, ICustomCondition> cachedConditions);
    }
}
