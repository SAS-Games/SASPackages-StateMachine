# State Machine Refactor Notes

## Current Refactor State

- Runtime transition execution was moved from `State`-only targets to a common `ITransitionNode`.
- `State`, `SubStateMachine`, `EntryNode`, and `ExitNode` can participate in transitions.
- `StateMachineModel` now builds real nested `RuntimeStateGraph` instances instead of treating child state machines as purely visual containers.
- `StateTransitionModel` now stores generic `TransitionNodeModel` endpoints with legacy `StateModel` source/target fields kept as fallback for old assets.
- Every state machine model can own `EntryStateModel` and `ExitStateModel`.
- Root Entry is synchronized in the editor to always point to the controller default state, matching Unity Animator-style behavior.
- Child/sub state machine Entry nodes remain user-controlled.

## Important Runtime Semantics

- Entering a sub state machine enters its child graph through Entry.
- Transitioning to a child graph Exit marks that child graph complete.
- Once complete, the parent `SubStateMachine` node evaluates its own outgoing transitions.
- If a submachine reaches Exit and has no valid parent outgoing transition, it remains completed until one becomes valid.
- `Actor.CurrentState` still reports the active leaf `State` for compatibility.

## Debugger / Breakpoint Idea

The new transition-node architecture is suitable for debugger support.

Potential design:

- Add a stable debug identity to every `ITransitionNode` / `TransitionNodeModel`.
- Fire runtime debug events on:
  - node entered
  - node exited
  - transition evaluated
  - transition taken
  - transition blocked by conditions
  - action executed
- Store editor breakpoints against `TransitionNodeModel` asset references.
- When a breakpoint node is entered, pause play mode or pause only this state machine runner.
- In the editor, select/focus the active node and show:
  - current node path, including parent submachines
  - transition conditions and live parameter values
  - state action list
  - action instance runtime values, where inspectable
  - current awaitable action completion state

Likely implementation points:

- `RuntimeStateGraph.SetCurrentNode(...)` for node-enter breakpoints.
- `State.OnEnter()`, `State.OnExit()`, and action execution loops for action-level inspection.
- `TransitionNodeUtility.TryGetNextNode(...)` / `TransitionState.TryGetTransition(...)` for condition tracing.
- `StateMachineEditorWindow.SetCurrentActiveNode()` for focusing the editor on the live node.

Open design question:

- Decide whether a breakpoint should pause all Unity play mode via `EditorApplication.isPaused`, or only pause this state machine while the game continues.
