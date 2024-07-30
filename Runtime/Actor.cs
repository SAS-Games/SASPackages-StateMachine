using SAS.StateMachineGraph.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public sealed class Actor : MonoBehaviour, IActivatable
    {
        [Serializable]
        public struct Config
        {
            public ScriptableObject data;
            public string name;
        }

        [SerializeField] private RuntimeStateMachineController m_Controller = default;
        [SerializeField] private Config[] m_Configs = default;
        [SerializeField] private bool m_AutoInitialize = true;

        internal StateMachine StateMachineController { get; private set; }
        public string CurrentStateName => StateMachineController?.CurrentState?.Name;
        public State CurrentState => StateMachineController?.CurrentState;

        private Dictionary<string, List<object>> _configs = new Dictionary<string, List<object>>();
        private bool _isConfigsCached = false;
        private bool _initialized = false;
        internal bool _isActiveAndEnabled = false;

        public RuntimeStateMachineController runtimeStateMachineController
        {
            get => m_Controller;
            set
            {
                if (m_Controller != value)
                {
                    _initialized = false;
                    m_Controller = value;
                }
                Initialize();
            }
        }

        private void Awake()
        {
            _isActiveAndEnabled = true;
            ActorEarlyUpdateManager.Register(this);
            if (m_AutoInitialize)
                Initialize();
        }

        void OnDestroy()
        {
            ActorEarlyUpdateManager.Unregister(this);
        }

        void OnEnable()
        {
            _isActiveAndEnabled = true;
        }

        void OnDisable()
        {
            _isActiveAndEnabled = false;
        }

        private void Start()
        {
            StateMachineController.nextState = StateMachineController.DefaultState;
            StateMachineController.CurrentState = StateMachineController.DefaultState;
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

        public void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;
            var controller = ScriptableObject.CreateInstance<RuntimeStateMachineController>();
            if (m_Controller == null)
                return;
            var stateMachineOverrideController = m_Controller as StateMachineOverrideController;
            m_Controller = (m_Controller is StateMachineOverrideController) ? stateMachineOverrideController.runtimeStateMachineController : m_Controller;

            controller.Initialize(m_Controller);
            m_Controller = controller;
            StateMachineController = m_Controller?.CreateStateMachine(this, stateMachineOverrideController);
        }

        public void EarlyUpdate()
        {
            StateMachineController?.OnEarlyUpdate();
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
            StateMachineController?.TryTransition();
        }

        public void SetFloat(string name, float value)
        {
            StateMachineController?.SetFloat(name, value);
        }

        public void SetFloat(int id, float value)
        {
            StateMachineController?.SetFloat(id, value);
        }

        public void SetInteger(string name, int value)
        {
            StateMachineController?.SetInteger(name, value);
        }

        public void SetInteger(int id, int value)
        {
            StateMachineController?.SetInteger(id, value);
        }

        public void SetBool(string name, bool value)
        {
            StateMachineController?.SetBool(name, value);
        }

        public void SetBool(int id, bool value)
        {
            StateMachineController?.SetBool(id, value);
        }

        public void SetTrigger(string name)
        {
            StateMachineController?.SetTrigger(name);
        }

        public void SetTrigger(int id)
        {
            StateMachineController?.SetTrigger(id);
        }

        public void ResetSetTrigger(string name)
        {
            StateMachineController?.ResetSetTrigger(name);
        }

        public void ResetSetTrigger(int id)
        {
            StateMachineController?.ResetSetTrigger(id);
        }

        public int GetInteger(string name)
        {
            return StateMachineController.GetInteger(name);
        }

        public int GetInteger(int id)
        {
            return StateMachineController.GetInteger(id);
        }

        public float GetFloat(string name)
        {
            return StateMachineController.GetFloat(name);
        }

        public float GetFloat(int id)
        {
            return StateMachineController.GetFloat(id);
        }

        public bool GetBool(string name)
        {
            return StateMachineController.GetBool(name);
        }

        public bool GetBool(int id)
        {
            return StateMachineController.GetBool(id);
        }

        public State GetState(string name)
        {
            return StateMachineController.GetStateByName(name);
        }

        public State GetStateByTag(string name)
        {
            return StateMachineController.GetStateByTag(name);
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

        public bool TryGet<T>(out T config, string key = "") where T : ScriptableObject
        {
            CacheConfig();
            var status = TryGet(typeof(T), out var result, key);
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

        public void SetState(string name)
        {
            StateMachineController.SetState(name);
        }
    }
}