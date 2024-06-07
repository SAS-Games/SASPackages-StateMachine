namespace SAS.StateMachineGraph
{
	public interface ICustomCondition
	{
		void OnInitialize(Actor actor);
		void OnStateEnter();
		void OnStateExit();
		bool Evaluate();
	}
}
