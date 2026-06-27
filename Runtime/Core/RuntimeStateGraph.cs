namespace SAS.StateMachineGraph
{
    public sealed class RuntimeStateGraph
    {
        private const int MaxTransientTransitionDepth = 32;

        internal RuntimeStateGraph(StateMachine stateMachine, string name)
        {
            StateMachine = stateMachine;
            Name = name;
        }

        internal StateMachine StateMachine { get; }
        internal string Name { get; }
        internal ITransitionNode EntryNode { get; set; }
        internal ITransitionNode ExitNode { get; set; }
        internal ITransitionNode DefaultNode { get; set; }
        internal RuntimeStateGraph ParentGraph { get; set; }
        internal SubStateMachine OwnerSubStateMachine { get; set; }
        internal ITransitionNode CurrentNode { get; private set; }
        internal ITransitionNode NextNode { get; set; }
        internal bool IsComplete { get; private set; }
        internal State CurrentState => CurrentNode?.ActiveState;

        internal void Enter()
        {
            IsComplete = false;
            NextNode = null;
            SetCurrentNode(EntryNode ?? DefaultNode);
            ResolveTransientNodes();
        }

        internal void SetCurrentNode(ITransitionNode node)
        {
            CurrentNode = node;
            CurrentNode?.OnEnter();
            IsComplete = CurrentNode == ExitNode;
        }

        internal void QueueTransition(ITransitionNode node)
        {
            NextNode = node;
        }

        internal void OnEarlyUpdate()
        {
            ApplyQueuedTransition();
            CurrentNode?.OnEarlyUpdate();
            ResolveTransientNodes();
        }

        internal void OnFixedUpdate()
        {
            if (!IsComplete)
                CurrentNode?.OnFixedUpdate();
        }

        internal void OnUpdate()
        {
            if (!IsComplete)
                CurrentNode?.OnUpdate();
        }

        internal void OnLateUpdate()
        {
            if (!IsComplete)
                CurrentNode?.OnLateUpdate();
        }

        internal void TryTransition()
        {
            if (!IsComplete)
                CurrentNode?.TryTransition();
        }

        internal void ResetTrigger()
        {
            CurrentNode?.ResetTrigger();
        }

        private void ApplyQueuedTransition()
        {
            if (NextNode == null)
                return;

            SetCurrentNode(NextNode);
            NextNode = null;
        }

        private void ResolveTransientNodes()
        {
            int guard = 0;
            while (CurrentNode is EntryNode && guard++ < MaxTransientTransitionDepth)
            {
                CurrentNode.TryTransition();
                if (NextNode == null)
                    break;

                ApplyQueuedTransition();
            }
        }
    }
}
