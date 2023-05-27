using SAS.Utilities.TagSystem;

namespace SAS.StateMachineGraph.Utilities
{
	public class ApplyActorParameter : IStateAction
	{
		private Actor[] _actors;
		private string _key; 

		void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
		{
			actor.TryGetComponentsInChildren(out _actors, tag, true);
			_key = key;
		}

		void IStateAction.Execute(ActionExecuteEvent executeEvent)
		{
			for (int i = 0; i < _actors.Length; ++i)
			{
				if (_actors[i].TryGet(out ActorParameterConfig parameterConfig))
					parameterConfig.ApplyParameters(_actors[i], _key);
			}
		}
	}
}
