using System;
using System.Collections.Generic;
using UnityEngine;
using SAS.TagSystem;
using SAS.Locator;

namespace SAS.StateMachineGraph
{
    public class Actor : MonoBehaviour
    {
        [SerializeField] private StateMachineModel m_Controller = default;

        internal StateMachine StateMachineController { get; private set; }
        private readonly Dictionary<Type, Component> _cachedComponents = new Dictionary<Type, Component>();
        private readonly ServiceLocator _serviceLocator = new ServiceLocator();
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

        public bool TryGet<T>(out T component, bool includeInactive = false) where T : Component
        {
            return TryGet<T>(string.Empty, out component, includeInactive);
        }

        public bool TryGet<T>(string tag, out T component, bool includeInactive = false) where T : Component
        {
            if (!_serviceLocator.TryGet<T>(out component, tag))
                component = this.GetComponentInChildren<T>(tag, includeInactive);
            if (component != null)
                _serviceLocator.Add<T>(component, tag);

            return component != null;
        }

        public bool TryGetAll<T>(string tag, out T[] components, bool includeInactive = false) where T : Component
        {
            // if (!_serviceLocator.TryGet<T>(out component, tag))
            components = this.GetComponentsInChildren<T>(tag, includeInactive);
          //  if (component != null)
          //      _serviceLocator.Add<T>(component, tag);

            return components != null;
        }

        public void SetFloat(string name, float value)
        {
            StateMachineController?.SetFloat(name, value);
        }

        public void SetInt(string name, int value)
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
    }
}
