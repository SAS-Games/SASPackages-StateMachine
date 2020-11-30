using System;
using System.Collections.Generic;
using UnityEngine;
using SAS.TagSystem;

namespace SAS.StateMachineGraph
{
    public class Actor : MonoBehaviour
    {
        [SerializeField] private StateMachineModel m_Controller = default;
        
        internal StateMachine StateMachineController { get; private set; }
        private readonly Dictionary<Type, Component> _cachedComponents = new Dictionary<Type, Component>();
        public string CurrentStateName => StateMachineController?.CurrentState.Name;

        private void Awake()
        {
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

        public new bool TryGetComponent<T>(out T component) where T : Component
        {
            var type = typeof(T);
            if (!_cachedComponents.TryGetValue(type, out var value))
            {
                if (base.TryGetComponent<T>(out component))
                    _cachedComponents.Add(type, component);

                return component != null;
            }

            component = (T)value;
            return true;
        }

        public T GetOrAddComponent<T>() where T : Component
        {
            if (!TryGetComponent<T>(out var component))
            {
                component = gameObject.AddComponent<T>();
                _cachedComponents.Add(typeof(T), component);
            }

            return component;
        }

        public new T GetComponent<T>() where T : Component
        {
            return TryGetComponent(out T component)
                ? component : throw new InvalidOperationException($"{typeof(T).Name} not found in {name}.");
        }

        public void SetFloat(string name, float value)
        {
            StateMachineController?.SetFloat(name, value);
        }
        public void SetBool(string name, bool value)
        {
            StateMachineController?.SetBool(name, value);
        }
    }
}
