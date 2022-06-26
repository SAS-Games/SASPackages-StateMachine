
namespace SAS.StateMachineGraph
{
    public interface IStateAction 
    {
        void OnInitialize(Actor actor, string tag, string key, State state);
        void Execute();
    }

    public interface IAwaitableStateAction : IStateAction
    {
        bool IsCompleted { get; }
    }
}
