using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
	public class ApplyAnimatorParameter : IStateAction
	{
		ParameterConfigBase.ParametersKeyMap _parameter;
		private Animator[] _animators;

		void IStateAction.OnInitialize(Actor actor, string tag, string key, State state)
		{
			actor.TryGetComponentInChildren(out _animators, tag, true);
			if (actor.TryGet(out AnimatorParameterConfig parameterConfig, key))
				_parameter = parameterConfig.Get(key);
		}

		void IStateAction.Execute(Actor actor)
		{
			for (int i = 0; i < _animators.Length; ++i)
				ApplyParameters(_animators[i]);
		}

		private void ApplyParameters(Animator animator)
		{
			for (int i = 0; i < _parameter.parameters.Length; ++i)
				animator.Apply(_parameter.parameters[i]);
		}
	}
}
