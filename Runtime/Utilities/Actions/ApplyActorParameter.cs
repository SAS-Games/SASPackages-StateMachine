
using UnityEngine;
using static SAS.StateMachineGraph.Utilities.ParameterConfigBase;

namespace SAS.StateMachineGraph.Utilities
{
	public class ApplyActorParameter : IStateAction
	{
		private Actor[] _actors;
		private string _key; 

		void IStateAction.OnInitialize(Actor actor, string tag, string key, State state)
		{
			actor.TryGetComponentsInChildren(out _actors, tag, true);
			_key = key;
		}

		void IStateAction.Execute(Actor actor)
		{
			for (int i = 0; i < _actors.Length; ++i)
			{
				if (_actors[i].TryGet(out ActorParameterConfig parameterConfig))
				{
					var parametersKeyMap = parameterConfig.Get(_key);
					if (parametersKeyMap != null)
						ApplyParameters(_actors[i], parametersKeyMap.parameters);
					else
						Debug.LogWarning($"No parameters config has been found wrt the key:  {_key} for actor: {actor.name}.");
				}
			}
		}

		private void ApplyParameters(Actor actor, in Parameter[] parameters)
		{
			for (int i = 0; i < parameters.Length; ++i)
				actor.Apply(parameters[i]);
		}
	}
}
