
namespace SAS.StateMachineGraph
{
    public interface IStateAction
    {
        void OnInitialize(Actor actor, string tag, string key);
        void Execute(ActionExecuteEvent executeEvent);
    }

    public interface IAwaitableStateAction : IStateAction
    {
        bool IsCompleted { get; }
    }
}
