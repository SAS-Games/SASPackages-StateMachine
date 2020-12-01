using UnityEngine;
using System;
using System.Collections.Generic;

namespace SAS.StateMachineGraph
{ 

    [Serializable]
    internal class StateActionModel : SerializedType
    {
        internal string Name => Sanitize(ToType().ToString());
		[SerializeField] internal string tag;
        public override bool Equals(object obj)
        {
			return Name.Equals((obj as StateActionModel).Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        internal IStateAction GetAction(StateMachine stateMachine, Dictionary<StateActionModel, object> createdInstances)
		{
			if (createdInstances.TryGetValue(this, out var obj))
				return (IStateAction)obj;
			Type type = ToType();
            IStateAction action = null;
            if (type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                var component = stateMachine.Actor.GetComponentInChildren(type, true);
                if (component != null)
                    action = component as IStateAction;
                else
                {
                    Debug.LogError($"Mono Action {type} with tag {tag}  is not found!  Try attaching it under Actor {stateMachine.Actor}");
                    return action;
                }
            }
            else
                action = Activator.CreateInstance(ToType()) as IStateAction;

			createdInstances.Add(this, action);
            (action as IStateActionInit)?.OnInitialize(stateMachine.Actor);
			return action;
		}
	}
}
