
namespace SAS.StateMachineGraph
{
    public interface IStateAction { }

    public interface IStateInitialize : IStateAction
    {
        void OnInitialize(Actor actor);
    }

    public interface IStateEnter : IStateAction
    {
        void OnStateEnter(Actor actor);
    }

    public interface IStateFixedUpdate : IStateAction
    {
        void OnFixedUpdate(Actor actor);
    }

    public interface IStateUpdate : IStateAction
    {
        void OnUpdate(Actor actor);
    }

    public interface IStateExit : IStateAction
    {
        void OnStateExit(Actor actor);
    }
}
