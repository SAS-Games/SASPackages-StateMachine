using UnityEngine;
using SAS.Utilities;

namespace SAS.StateMachineGraph.Utilities
{
	public class ApplyAnimatorParameter : IStateInitialize, IStateEnter, IStateExit
	{
		ParameterConfig.ParametersKeyMap _parameter;
		private Animator _animator;

		void IStateInitialize.OnInitialize(Actor actor, string tag, string key)
		{
			actor.TryGetComponentInChildren(out _animator, includeInactive: true);
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
				_animator.Apply(_parameter.parameters[i]);
		}
	}
}
