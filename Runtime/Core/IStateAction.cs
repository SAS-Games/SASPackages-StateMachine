
namespace SAS.StateMachineGraph
{
    public interface IStateAction { }

    public interface IStateActionInit : IStateAction
    {
        void OnInitialize(Actor actor);
    }

    public interface IStateEnterAction : IStateAction
    {
        void OnStateEnter(Actor actor);
    }

    public interface IStateFixedUpdateAction : IStateAction
    {
        void OnFixedUpdate(Actor actor);
    }

    public interface IStateUpdateAction : IStateAction
    {
        void OnUpdate(Actor actor);
    }

    public interface IStateExitAction : IStateAction
    {
        void OnStateExit(Actor actor);
    }
}
