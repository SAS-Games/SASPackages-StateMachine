﻿using UnityEngine;
using System.Collections.Generic;

namespace SAS.StateMachineGraph
{
    internal class StateMachine
    {
        private Dictionary<string, StateMachineParameter> _parameters = new Dictionary<string, StateMachineParameter>();
        public StateMachine(Actor actor, StateMachineParameter[] parameters)
        {
            Actor = actor;
            foreach (StateMachineParameter parameter in parameters)
                _parameters.Add(parameter.name, parameter);
        }

        public Actor Actor { get; }

        internal State DefaultState { get; set; }

        private State _currentState;
        internal State CurrentState
        {
            get => _currentState;
            set
            {
                _currentState?.OnExit();
                _currentState = value;
                _currentState?.OnEnter();
                _currentState?.AwaitableStateAction();
            }
        }

        internal State AnyState { get; set; }

        internal void OnUpdate()
        {
            CurrentState?.OnUpdate();
        }

        internal void OnFixedUpdate()
        {
            CurrentState?.OnFixedUpdate();
        }

        internal void AwaitableStateAction()
        {
            CurrentState?.AwaitableStateAction();
        }

        internal void TryTransition()
        {
            CurrentState?.TryTransition();
            AnyState?.TryTransition();
        }

        public int GetInt(string name)
        {
            try
            {
                return _parameters[name].IntValue;
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogException(e);
                return 0;
            }
        }

        public float GetFloat(string name)
        {
            try
            {
                return _parameters[name].FloatValue;
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogException(e);
                return 0f;
            }
        }

        public bool GetBool(string name)
        {
            try
            {
                return _parameters[name].BoolValue;
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public void SetInt(string name, int value)
        {
            try
            {
                _parameters[name].IntValue = value;
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogException(e);
            }
        }

        public void SetFloat(string name, float value)
        {
            try
            {
                _parameters[name].FloatValue = value;
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogException(e);
            }
        }

        public void SetBool(string name, bool value)
        {
            try
            {
                _parameters[name].BoolValue = value;
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogException(e);
            }
        }

        public void SetTrigger(string name)
        {
            try
            {
                _parameters[name].BoolValue = true;
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogException(e);
            }
        }

        public void ResetSetTrigger(string name)
        {
            try
            {
                _parameters[name].BoolValue = false;
            }
            catch (KeyNotFoundException e)
            {
                Debug.LogException(e);
            }
        }
    }
}
