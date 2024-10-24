using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    internal class StateMachine
    {
        private Dictionary<int, StateMachineParameter> _parameters = new Dictionary<int, StateMachineParameter>();
        internal List<StateActionPair> stateActionPairs;
        internal List<State> states = new List<State>();

        public StateMachine(Actor actor, StateMachineParameter[] parameters, List<StateActionPair> stateActionPairs)
        {
            Actor = actor;
            this.stateActionPairs = stateActionPairs;
            foreach (StateMachineParameter parameter in parameters)
                _parameters.Add(Animator.StringToHash(parameter.name), parameter);
        }

        public Actor Actor { get; }

        internal State DefaultState { get; set; }

        private State _currentState;
        internal State nextState;

        internal State CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                _currentState?.OnEnter();
            }
        }

        internal void OnEarlyUpdate()
        {
            if (_currentState != nextState && nextState != null)
                CurrentState = nextState;
        }

        internal void OnFixedUpdate()
        {
            CurrentState.OnFixedUpdate();
        }

        internal State AnyState { get; set; }

        internal void OnUpdate()
        {
            CurrentState.OnUpdate();
        }

        internal void OnLateUpdate()
        {
            CurrentState.OnLateUpdate();
        }

        internal void TryTransition()
        {
            CurrentState.TryTransition();
            AnyState.TryTransition();
            CurrentState.ResetTrigger();
            AnyState.ResetTrigger();

        }

        public int GetInteger(string name)
        {
            return GetInteger(Animator.StringToHash(name));
        }

        public int GetInteger(int id)
        {
            if (_parameters.TryGetValue(id, out StateMachineParameter parameter))
                return parameter.IntValue;

            Debug.LogError($"Parameter hash {id} not defined for the actor {this.Actor.name}");
            return 0;

        }

        public float GetFloat(string name)
        {
            return GetFloat(Animator.StringToHash(name));
        }

        public float GetFloat(int id)
        {
            if (_parameters.TryGetValue(id, out StateMachineParameter parameter))
                return parameter.FloatValue;

            Debug.LogError($"Parameter hash {id} not defined for the actor {this.Actor.name}");
            return 0;
        }

        public bool GetBool(string name)
        {
            return GetBool(Animator.StringToHash(name));
        }

        public bool GetBool(int id)
        {
            if (_parameters.TryGetValue(id, out StateMachineParameter parameter))
                return parameter.BoolValue;

            Debug.LogError($"Parameter hash {id} not defined for the actor {this.Actor.name}");
            return false;
        }

        public void SetInteger(string name, int value)
        {
            SetInteger(Animator.StringToHash(name), value);
        }

        public void SetInteger(int id, int value)
        {
            if (_parameters.TryGetValue(id, out StateMachineParameter parameter))
                parameter.IntValue = value;
            else
                Debug.LogError($"Parameter hash {id} not defined for the actor {this.Actor.name}");
        }

        public void SetFloat(string name, float value)
        {
            SetFloat(Animator.StringToHash(name), value);
        }

        public void SetFloat(int id, float value)
        {
            if (_parameters.TryGetValue(id, out StateMachineParameter parameter))
                parameter.FloatValue = value;
            else
                Debug.LogError($"Parameter hash {id} not defined for the actor {this.Actor.name}");
        }

        public void SetBool(string name, bool value)
        {
            SetBool(Animator.StringToHash(name), value);
        }

        public void SetBool(int id, bool value)
        {
            if (_parameters.TryGetValue(id, out StateMachineParameter parameter))
                parameter.BoolValue = value;
            else
                Debug.LogError($"Parameter hash {id} not defined for the actor {this.Actor.name}");
        }

        public void SetTrigger(string name)
        {
            SetTrigger(Animator.StringToHash(name));
        }

        public void SetTrigger(int id)
        {
            if (_parameters.TryGetValue(id, out StateMachineParameter parameter))
                parameter.BoolValue = true;
            else
                Debug.LogError($"Parameter hash {id} not defined for the actor {this.Actor.name}");
        }

        public void ResetSetTrigger(string name)
        {
            ResetSetTrigger(Animator.StringToHash(name));
        }

        public void ResetSetTrigger(int id)
        {
            if (_parameters.TryGetValue(id, out StateMachineParameter parameter))
                parameter.BoolValue = false;
            else
                Debug.LogError($"Parameter hash {id} not defined for the actor {this.Actor.name}");
        }

        internal State GetStateByName(string name)
        {
            return states.Find(x => x.Name == name);
        }

        internal State GetStateByTag(string tag)
        {
            return states.Find(x => x.Tag == tag);
        }

        internal void SetState(string stateName)
        {
            var state = GetStateByName(stateName);
            if (state != null)
            {
                CurrentState = state;
                nextState = state;
            }
            else
                Debug.LogError($"No State Found with name {stateName}");
        }
    }
}