using SAS.Utilities.TagSystem;
namespace SAS.StateMachineGraph.Utilities
{
    public sealed class DeactivateObjects: IStateAction
    {
        private IActivatable[] _activatables;
        
        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            actor.TryGetComponentsInChildren(out _activatables, tag, true);
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            foreach (IActivatable activatable in _activatables)
                activatable.Deactivate();
        }
    }
}
