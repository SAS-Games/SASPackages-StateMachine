
namespace SAS.StateMachineGraph
{
  /*  interface IStateAction
    {
        void Execute();
    }*/

    public interface IStateAction
    {
        void Init(Actor actor);
        void Execute(Actor actor);
    }
}
