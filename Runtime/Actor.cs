using System;
using UnityEngine;
using SAS.TagSystem;
using SAS.StateMachineGraph.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SAS.StateMachineGraph
{
    public sealed class Actor : MonoBehaviour, IActivatable
    {
        internal delegate void StateChanged(State state, bool entered);
        private StateChanged OnStateChanged;

        public Action<State> OnStateEnter;
        public Action<State> OnStateExit;

        [Serializable]
        public struct Config
        {
            public ScriptableObject data;
            public string name;
        }

        [SerializeField] private RuntimeStateMachineController m_Controller = default;
        [SerializeField] private Config[] m_Configs = default;

        internal StateMachine StateMachineController { get; private set; }
        public string CurrentStateName => StateMachineController?.CurrentState?.Name;
        public State CurrentState => StateMachineController?.CurrentState;

        private Dictionary<string, List<object>> _configs = new Dictionary<string, List<object>>();
        private bool _isConfigsCached = false;

        private void Awake()
        {
            OnStateChanged = InvokeEvent;
            Initialize();
        }

        private void CacheConfig()
        {
            if (!_isConfigsCached)
            {
                foreach (var config in m_Configs)
                    Add(config.data.GetType(), config.data, config.name);
                _isConfigsCached = true;
            }
        }


        private void Initialize()
        {
            var controller = ScriptableObject.CreateInstance<RuntimeStateMachineController>();
            if (m_Controller == null)
                return;
            var stateMachineOverrideController = m_Controller as StateMachineOverrideController;
            m_Controller = (m_Controller is StateMachineOverrideController) ? stateMachineOverrideController.runtimeStateMachineController : m_Controller;
            
            controller.Initialize(m_Controller);
            m_Controller = controller;
            StateMachineController = m_Controller?.CreateStateMachine(this, stateMachineOverrideController);
        }

        private void FixedUpdate()
        {
            StateMachineController?.OnFixedUpdate();
        }

        private void Update()
        {
            StateMachineController?.OnUpdate();
        }

        private void LateUpdate()
        {
            StateMachineController?.OnLateUpdate();
            StateMachineController?.TryTransition(OnStateChanged);
        }

        public void SetFloat(string name, float value)
        {
            StateMachineController?.SetFloat(name, value);
        }

        public void SetInteger(string name, int value)
        {
            StateMachineController?.SetInteger(name, value);
        }

        public void SetBool(string name, bool value)
        {
            StateMachineController?.SetBool(name, value);
        }

        public void SetTrigger(string name)
        {
            StateMachineController?.SetTrigger(name);
        }

        public void ResetSetTrigger(string name)
        {
            StateMachineController?.ResetSetTrigger(name);
        }

        public int GetInteger(string name)
        {
            return StateMachineController.GetInteger(name);
        }

        public float GetFloat(string name)
        {
            return StateMachineController.GetFloat(name);
        }

        public bool GetBool(string name)
        {
            return StateMachineController.GetBool(name);
        }

        void IActivatable.Activate()
        {
            enabled = true;
        }

        void IActivatable.Deactivate()
        {
            enabled = false;
        }

        public void Apply(in Parameter parameter)
        {
            switch (parameter.Type)
            {
                case ParameterType.Bool:
                    SetBool(parameter.Name, parameter.BoolValue);
                    break;
                case ParameterType.Int:
                    SetInteger(parameter.Name, parameter.IntValue);
                    break;
                case ParameterType.Float:
                    SetFloat(parameter.Name, parameter.FloatValue);
                    break;
                case ParameterType.Trigger:
                    if (!parameter.BoolValue)
                        SetTrigger(parameter.Name);
                    else 
                        ResetSetTrigger(parameter.Name);
                    break;
            }
        }

        private void InvokeEvent(State state, bool isStateEntered)
        {
            if (isStateEntered)
                OnStateEnter?.Invoke(state);
            else
                OnStateExit?.Invoke(state);
        }

        public bool TryGet<T>(out T config, string tag = "") where T : ScriptableObject
        {
            CacheConfig();
            var status = TryGet(typeof(T), out var result, tag);
            config = (T)result;
            return status;
        }

        private bool TryGet(Type type, out object config, string tag = "")
        {
            var key = $"{type.Name}{tag}";
            if (!_configs.TryGetValue(key, out var configs))
            {
                config = null;
                return false;
            }

            if (configs.Count > 1)
                Debug.LogError($"There is more than one Config that implements {type.Name} with tag {tag} is found under actor {name}");

            config = configs[0];
            return true;
        }

        private void Add(Type type, object service, string tag = "")
        {
            var key = $"{type.Name}{tag}";
            if (!_configs.TryGetValue(key, out var serviceList))
            {
                serviceList = new List<object>();
                _configs.Add(key, serviceList);
            }

            if (!serviceList.Contains(service))
                serviceList.Add(service);

            var baseTypes = type.GetInterfaces();
            if (type.BaseType != null)
                baseTypes = baseTypes.Prepend(type.BaseType).ToArray();

            foreach (var baseType in baseTypes)
                Add(baseType, service, tag);
        }
    }
}
