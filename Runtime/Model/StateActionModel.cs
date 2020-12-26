using UnityEngine;
using System;
using System.Collections.Generic;
using SAS.TagSystem;

namespace SAS.StateMachineGraph
{ 

    [Serializable]
    internal class StateActionModel : SerializedType
    {
        internal string Name => Sanitize(ToType().ToString());
		[SerializeField] internal string tag = default;
        public override bool Equals(object obj)
        {
			return Name.Equals((obj as StateActionModel).Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() + tag.GetHashCode();
        }

        internal IStateAction[] GetActions(StateMachine stateMachine, Dictionary<StateActionModel, object[]> createdInstances)
        {
            IStateAction[] stateActions;
            if (createdInstances.TryGetValue(this, out var actions))
                return actions as IStateAction[];
            Type type = ToType();
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                actions = stateMachine.Actor.GetComponentsInChildren(type, tag, true);
                if (actions != null)
                {
                    stateActions = new IStateAction[actions.Length];
                    for (int i = 0; i < actions.Length; i++)
                        stateActions[i] = actions[i] as IStateAction;
                }
                else
                {
                    Debug.LogError($"Mono Action {type} with tag {tag}  is not found!  Try attaching it under Actor {stateMachine.Actor}");
                    return null;
                }
            }
            else
                stateActions = new IStateAction[] { Activator.CreateInstance(ToType()) as IStateAction };

            createdInstances.Add(this, stateActions);
            foreach (var action in stateActions)
                (action as IStateInitialize)?.OnInitialize(stateMachine.Actor);
            return stateActions;
        }
	}
}
