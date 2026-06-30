namespace SAS.StateMachineGraph
{
    public enum StateMachineDebugEventType
    {
        NodeEnter,
        NodeExit,
        BeforeAction,
        AfterAction,
        BeforeTransitionEvaluation,
        AfterTransitionEvaluation,
        TransitionTaken
    }
}
