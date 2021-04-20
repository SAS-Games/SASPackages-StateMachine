using System;
using UnityEngine;
using SAS.TagSystem;
using SAS.Locator;
using SAS.StateMachineGraph.Utilities;
using SAS.Utilities;

namespace SAS.StateMachineGraph
{
    public sealed class Actor : MonoBehaviour, IActivatable
    {
        internal delegate void StateChanged(string stateName, bool entered);
        private StateChanged OnStateChanged;

        public Action<string> OnStateEnter;
        public Action<string> OnStateExit;

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
            OnStateChanged = InvokeEvent;

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
            StateMachineController?.TryTransition(OnStateChanged);
        }

        public T Get<T>(string tag = "", bool includeInactive = false)
        {
            if (TryGet<T>(out var service, tag, includeInactive))
                return service;
            return default(T);
        }


        public bool TryGet<T>(out T service, string tag = "", bool includeInactive = false)
        {
            if (!_serviceLocator.TryGet<T>(out service, tag))
            {
                if (typeof(T).IsSubclassOf(typeof(Component)))
                    return TryGetComponent(out service, tag, includeInactive);
            }
            return true;
        }

        private bool TryGetComponent<T>(out T component, string tag, bool includeInactive)
        {
            component = (T)(object)GetComponent(typeof(T), tag, includeInactive);
            return component != null;
        }

        public bool TryGetComponents<T>(string tag, out T[] components, bool includeInactive = false)
        {
            var results = this.GetComponentsInChildren(typeof(T), tag, includeInactive);
            try
            {
                if (results.Length == 0)
                    results = null;

                components = new T[results.Length];
                for (int i = 0; i < results.Length; ++i)
                    components[i] = (T)(object)results[i];
            }
            catch (Exception)
            {
                components = null;
                Debug.LogError($"No component of type {components.GetType()} with tag {tag} is found under actor {this}, attached on the game object {gameObject.name}. Try assigning the component with the right Tag");
                return false;
            }

            return true;
        }

        private Component GetComponent(Type type, string tag, bool includeInactive)
        {
            if (!_serviceLocator.TryGet(type, out var obj, tag))
            {
                obj = this.GetComponentInChildren(type, tag, includeInactive);
                if (obj != null)
                    _serviceLocator.Add(type, obj, tag);
                else
                    Debug.LogError($"No component of type {type.GetType()} with tag {tag} is found under actor {this}, attached on the game object {gameObject.name}. Try assigning the component with the right Tag");
            }

            if (!includeInactive && !(obj as Behaviour).isActiveAndEnabled)
            {
                obj = null;
                Debug.LogError($"No active component of type {type.GetType()} with tag {tag} is found under actor {this}, attached on the game object {gameObject.name}. Try assigning the component with the right Tag and Make sure that the component is active");
            }

            return obj as Component;
        }

        public void SetFloat(string name, float value)
        {
            StateMachineController?.SetFloat(name, value);
        }

        public void SetInteger(string name, int value)
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
                    SetTrigger(parameter.Name);
                    break;
            }
        }

        private void InvokeEvent(string name, bool isStateEntered)
        {
            if (isStateEntered)
                OnStateEnter?.Invoke(name);
            else
                OnStateExit?.Invoke(name);
        }
    }
}
