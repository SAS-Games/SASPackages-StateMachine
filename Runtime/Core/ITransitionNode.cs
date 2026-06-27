namespace SAS.StateMachineGraph
{
    public interface ITransitionNode
    {
        string Name { get; }
        RuntimeStateGraph Graph { get; }
        State ActiveState { get; }
        TransitionState[] TransitionStates { get; set; }

        void OnEnter();
        bool OnExit();
        void OnEarlyUpdate();
        void OnFixedUpdate();
        void OnUpdate();
        void OnLateUpdate();
        void TryTransition();
        void ResetTrigger();
    }
}
