using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
	public class ApplyAnimatorParameter : IStateAction
	{
		private string _key;
		private Animator[] _animators;
		AnimatorParameterConfig parameterConfig;

		void IStateAction.OnInitialize(Actor actor, string tag, string key, State state)
		{
			actor.TryGetComponentInChildren(out _animators, tag, true);
			_key = key;
			actor.TryGet(out parameterConfig, key);
		}

		void IStateAction.Execute(Actor actor)
		{
			for (int i = 0; i < _animators.Length; ++i)
				parameterConfig.ApplyParameters(_animators[i], in _key);
		}
	}
}
