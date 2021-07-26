using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
	public class ApplyAnimatorParameter : IStateAction
	{
		ParameterConfig.ParametersKeyMap _parameter;
		private Animator _animator;

		void IStateAction.OnInitialize(Actor actor, string tag, string key, State state)
		{
			actor.TryGetComponentInChildren(out _animator, includeInactive: true);
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
				_animator.Apply(_parameter.parameters[i]);
		}
	}
}
