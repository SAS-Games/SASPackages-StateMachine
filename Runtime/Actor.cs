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
        private readonly ServiceLocator _serviceLocator = new ServiceLocator();
        public string CurrentStateName => StateMachineController?.CurrentState?.Name;

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

        public bool TryGet<T>(out T component, bool includeInactive = false)
        {
            return TryGet<T>(string.Empty, out component, includeInactive);
        }

        public bool TryGet<T>(string tag, out T component, bool includeInactive = false)
        {
            component = (T)(object)Get(typeof(T), tag, includeInactive);
            return component != null;
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
