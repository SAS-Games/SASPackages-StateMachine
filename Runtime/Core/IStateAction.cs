
namespace SAS.StateMachineGraph
{
    public interface IStateAction 
    {
        void OnInitialize(Actor actor, string tag, string key, State state);
        void Execute(Actor actor);
    }

    public interface IAwaitableStateAction : IStateAction
    {
        bool IsCompleted { get; }
    }
}
