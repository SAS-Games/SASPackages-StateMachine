using System;
using UnityEngine;
using SAS.StateMachineGraph;

namespace SAS.StateMachineGraph.Utilities
{
	public class ApplyActorParameter : IStateAction
	{
		private ParameterConfig.ParametersKeyMap _parameter;
		private Actor _actor;

		void IStateAction.OnInitialize(Actor actor, string tag, string key, State state)
		{
			actor.TryGetComponentInChildren(out _actor, includeInactive: true);
			if (actor.TryGet(out ParameterConfig parameterConfig, tag))
				_parameter = parameterConfig.Get(key);
		}

		void IStateAction.Execute(Actor actor)
		{
			ApplyParameters();
		}

		private void ApplyParameters()
		{
			for (int i = 0; i < _parameter.parameters.Length; ++i)
				_actor.Apply(_parameter.parameters[i]);
		}
	}
}
