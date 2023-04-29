# State Machine Tool README

The State Machine Tool is a tool that allows you to create state machines for running scripts in your project. It is similar to Unity Animator, but it allows you to attach actions to states using interfaces and choose when to run these actions based on the state's lifecycle.

## Getting Started

To get started, you will need to add the StateMachineTool package to your project. You can do this by using the unity package manager and this git hub packege into your project.

Add the following Packages
https://github.com/SAS-Games/SASPackages-Utilities.git
https://github.com/SAS-Games/SASPackages-StateMachine.git 

Once the package is imported, you can create a new state machine by right-clicking in the Project window and selecting "Create > State Machine Tool > State Machine". This will create a new state machine asset in your project.

To edit the state machine, you can double-click on the asset to open it in the State Machine Tool window.

## Creating States

To create a new state, you can right-click in the State Machine Tool window and select "Create State". This will create a new state that you can rename and configure.

You can also create sub-states by right-clicking on a state and selecting "Create Sub-State". Sub-states allow you to create hierarchies of states that can be used to organize your state machine.

## Attaching Actions

To attach an action to a state, you can click on the state to select it and then select an action from the "Actions" dropdown menu. Actions are implemented using the IStateAction or IAwaitableStateAction interface.

You can choose when to run an action by selecting one of the following options from the "When To Run" dropdown menu:
- OnStateEnter
- OnStateExit
- OnUpdate
- OnFixedUpdate

## Creating Custom Conditions

You can also create custom conditions by implementing the ICustomeCondition interface. To create a custom condition, you can right-click in the State Machine Tool window and select "Create Custom Condition". This will create a new asset that you can rename and configure.

You can then attach the custom condition to a transition by clicking on the transition and selecting the custom condition from the "Conditions" dropdown menu.

## Conclusion

The State Machine Tool is a powerful tool for creating state machines in your project. It allows you to easily attach actions and custom conditions to states and transitions, giving you full control over the behavior of your project.
<img width="1280" alt="StateMachine" src="https://user-images.githubusercontent.com/16848724/235297044-09b6af2d-a64c-4cf8-889a-029af02ad57a.png">
