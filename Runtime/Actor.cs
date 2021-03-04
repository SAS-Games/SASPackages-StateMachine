using System;
using UnityEngine;
using SAS.TagSystem;
using SAS.Locator;
using SAS.StateMachineGraph.Utilities;

namespace SAS.StateMachineGraph
{
    public sealed class Actor : MonoBehaviour, IActivatable
    {
        [Serializable]
        private struct Config
        {
            public string name;
            public ScriptableObject data;
        }

        [SerializeField] private StateMachineModel m_Controller = default;
        [SerializeField] private Config[] m_Configs;

        internal StateMachine StateMachineController { get; private set; }
        private readonly ServiceLocator _serviceLocator = new ServiceLocator();
        public string CurrentStateName => StateMachineController?.CurrentState?.Name;

        private void Awake()
        {
            foreach (var config in m_Configs)
                _serviceLocator.Add(config.data.GetType(), config.data, config.name);
            Initialize();
        }

        private void Initialize()
        {
            var controller = ScriptableObject.CreateInstance<StateMachineModel>();
            if (m_Controller == null)
                return;
            controller.Initialize(m_Controller);
            m_Controller = controller;
            StateMachineController = m_Controller?.CreateStateMachine(this);
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
            StateMachineController?.TryTransition();
        }

        public T Get<T>(string tag = "")
        {
            return _serviceLocator.Get<T>(tag);
        }
        public bool TryGet<T>(out T component, bool includeInactive = false)
        {
            return TryGet<T>(string.Empty, out component, includeInactive);
        }

        public bool TryGet<T>(string tag, out T component, bool includeInactive = false)
        {
            component = (T)(object)Get(typeof(T), tag, includeInactive);
            return component != null;
        }

        public bool TryGetAll<T>(string tag, out T[] components, bool includeInactive = false)
        {
            var results = this.GetComponentsInChildren(typeof(T), tag, includeInactive);
            try
            {
                components = new T[results.Length];
                for (int i = 0; i < results.Length; ++i)
                    components[i] = (T)(object)results[i];
            }
            catch (Exception)
            {
                components = null;
                return false;
            }

            return true;
        }

        public Component Get(Type type, bool includeInactive = false)
        {
            return Get(type, string.Empty, includeInactive);
        }

        public Component Get(Type type, string tag, bool includeInactive = false)
        {
            if (!_serviceLocator.TryGet(type, out var obj, tag))
            {
                obj = this.GetComponentInChildren(type, tag, includeInactive);
                if (obj != null)
                    _serviceLocator.Add(type, obj, tag);
            }

            return obj as Component;
        }

        public void SetFloat(string name, float value)
        {
            StateMachineController?.SetFloat(name, value);
        }

        public void SetIntger(string name, int value)
        {
            StateMachineController?.SetInt(name, value);
        }

        public void SetBool(string name, bool value)
        {
            StateMachineController?.SetBool(name, value);
        }

        public void SetTrigger(string name)
        {
            StateMachineController?.SetTrigger(name);
        }

        public int GetInt(string name)
        {
            return StateMachineController.GetInt(name);
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
                case StateMachineParameter.ParameterType.Bool:
                    SetBool(parameter.Name, parameter.BoolValue);
                    break;
                case StateMachineParameter.ParameterType.Int:
                    SetIntger(parameter.Name, parameter.IntValue);
                    break;
                case StateMachineParameter.ParameterType.Float:
                    SetFloat(parameter.Name, parameter.FloatValue);
                    break;
                case StateMachineParameter.ParameterType.Trigger:
                    SetTrigger(parameter.Name);
                    break;
            }
        }
    }
}
