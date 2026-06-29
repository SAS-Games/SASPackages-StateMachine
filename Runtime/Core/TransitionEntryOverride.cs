namespace SAS.StateMachineGraph
{
    internal readonly struct TransitionEntryOverride
    {
        internal TransitionEntryOverride(SubStateMachine subStateMachine, ITransitionNode entryTarget)
        {
            SubStateMachine = subStateMachine;
            EntryTarget = entryTarget;
        }

        internal SubStateMachine SubStateMachine { get; }
        internal ITransitionNode EntryTarget { get; }

        internal void Apply()
        {
            SubStateMachine?.SetEntryOverride(EntryTarget);
        }
    }
}
