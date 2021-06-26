﻿using System;
using UnityEngine;
using SAS.StateMachineGraph;

namespace SAS.StateMachineGraph.Utilities
{
	public class ApplyActorParameter : IStateInitialize, IStateEnter, IStateExit
	{
		private ParameterConfig.ParametersKeyMap _parameter;
		private Actor _actor;

		void IStateInitialize.OnInitialize(Actor actor, string tag, string key)
		{
			actor.TryGetComponentInChildren(out _actor, includeInactive: true);
			if (actor.TryGet(out ParameterConfig parameterConfig, tag))
				_parameter = parameterConfig.Get(key);
		}

		void IStateEnter.OnStateEnter(Actor actor)
		{
			if (_parameter.executeOnStateEnter)
				ApplyParameters();
		}

		void IStateExit.OnStateExit(Actor actor)
		{
			if (_parameter.executeOnStateExit)
				ApplyParameters();
		}

		private void ApplyParameters()
		{
			for (int i = 0; i < _parameter.parameters.Length; ++i)
				_actor.Apply(_parameter.parameters[i]);
		}
	}
}
