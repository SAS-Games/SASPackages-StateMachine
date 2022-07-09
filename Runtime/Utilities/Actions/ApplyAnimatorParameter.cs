using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
	public class ApplyAnimatorParameter : IStateAction
	{
		private string _key;
		private Animator[] _animators;
		AnimatorParameterConfig parameterConfig;

		void IStateAction.OnInitialize(Actor actor, string tag, string key)
		{
			actor.TryGetComponentsInChildren(out _animators, tag, true);
			_key = key;
			actor.TryGet(out parameterConfig);
		}

		void IStateAction.Execute(ActionExecuteEvent executeEvent)
		{
			for (int i = 0; i < _animators.Length; ++i)
				parameterConfig.ApplyParameters(_animators[i], in _key);
		}
	}
}
