using System;
using UnityEngine;
using SAS.TagSystem;
using SAS.Locator;
using SAS.StateMachineGraph.Utilities;

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
        private readonly ServiceLocator _serviceLocator = new ServiceLocator();
        public string CurrentStateName => StateMachineController?.CurrentState?.Name;
        public State CurrentState => StateMachineController?.CurrentState;
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
                    _serviceLocator.Add(config.data.GetType(), config.data, config.name);
                _isConfigsCached = true;
            }
        }

        private void Initialize()
        {
            var controller = ScriptableObject.CreateInstance<RuntimeStateMachineController>();
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
            StateMachineController?.OnLateUpdate();
            StateMachineController?.TryTransition(OnStateChanged);
        }

        public T Get<T>(string tag = "")
        {
            if (TryGet<T>(out var service, tag))
                return service;

            return default(T);
        }

        public void Add<T>(object service, string tag = "")
        {
            _serviceLocator.Add<T>(service, tag);
        }

        public bool TryGet<T>(out T service, string tag = "") 
        {
            CacheConfig();
            return _serviceLocator.TryGet<T>(out service, tag);
        }

        public T GetOrCreate<T>(string tag = "")
        {
            return _serviceLocator.GetOrCreate<T>(tag);
        }

        public bool Remove<T>(string tag = "")
        {
            return _serviceLocator.Remove<T>(tag);
        }

        public bool TryGetComponent<T>(out T component, string tag = "")
        {
            component = (T)(object)GetComponent(typeof(T), tag);
            return component != null;
        }

        public bool TryGetComponentInChildren<T>(out T component, string tag = "", bool includeInactive = false)
        {
            component = (T)(object)GetComponentInChildren(typeof(T), tag, includeInactive);
            return component != null;
        }

        public Component GetComponent(Type type, string tag = "")
        {
            var obj = TaggerExtensions.GetComponent(this, type, tag);
            if (obj == null)
                Debug.LogError($"No component of type {type.Name} with tag {tag} is found under actor {this}, attached on the game object {gameObject.name}. Try assigning the component with the right Tag");

            return obj;
        }

        public Component GetComponentInChildren(Type type, string tag = "", bool includeInactive = false)
        {
            var obj = TaggerExtensions.GetComponentInChildren(this, type, tag, includeInactive);
            if (obj == null)
                Debug.LogError($"No component of type {type.Name} with tag {tag} is found under actor {this}, attached on the game object {gameObject.name}. Try assigning the component with the right Tag");

            return obj;
        }

        public bool TryGetComponentsInChildren<T>(out T[] components, string tag, bool includeInactive = false)
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

        public bool TryGetComponentsInParent<T>(out T[] components, string tag = "", bool includeInactive = false)
        {
            var results = this.GetComponentsInParent(typeof(T), tag, includeInactive);
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
                Debug.LogError($"No component of type {components.GetType()} with tag {tag} is found in parent of actor {this}, attached on the game object {gameObject.name}. Try assigning the component with the right Tag");
                return false;
            }

            return true;
        }

        public bool TryGetComponentInParent<T>(out T component, string tag = "", bool includeInactive = false)
        {
            component = (T)(object)GetComponentInParent(typeof(T), tag, includeInactive);
            return component != null;
        }

        public Component GetComponentInParent(Type type, string tag = "", bool includeInactive = false)
        {
            var obj = TaggerExtensions.GetComponentInParent(this, type, tag, includeInactive);
            if (obj == null)
                Debug.LogError($"No component of type {type.GetType()} with tag {tag} is found under actor {this}, attached on the game object {gameObject.name}. Try assigning the component with the right Tag");

            return obj;
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
                    SetTrigger(parameter.Name);
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
    }
}
