using System;
using UnityEngine;
using SAS.StateMachineGraph;

namespace SAS.StateMachineGraph.Utilities
{
	public class ApplyActorParameter : IStateAction
	{
		private ParameterConfig.ParametersKeyMap _parameter;

		void IStateAction.OnInitialize(Actor actor, string tag, string key, State state)
		{
			if (actor.TryGet(out ParameterConfig parameterConfig, tag))
				_parameter = parameterConfig.Get(key);
		}

		void IStateAction.Execute(Actor actor)
		{
			ApplyParameters(actor);
		}

		private void ApplyParameters(Actor actor)
		{
			for (int i = 0; i < _parameter.parameters.Length; ++i)
				actor.Apply(_parameter.parameters[i]);
		}
	}
}
