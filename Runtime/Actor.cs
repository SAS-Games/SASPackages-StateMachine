using SAS.StateMachineGraph.Utilities;
using SAS.Utilities.BlackboardSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [DefaultExecutionOrder(-1)]
    public sealed class Actor : MonoBehaviour, IActivatable
    {
        [Serializable]
        public struct Config
        {
            public ScriptableObject data;
            public string name;
        }

        [SerializeField] private RuntimeStateMachineController m_Controller = default;
        [SerializeField] private BlackboardData m_BlackboardData = default;
        [SerializeField] private Config[] m_Configs = default;
        [SerializeField] private bool m_AutoInitialize = true;

        internal StateMachine StateMachineController { get; private set; }
        public string CurrentStateName => StateMachineController?.CurrentState?.Name;
        public State CurrentState => StateMachineController?.CurrentState;

        private Blackboard _blackboard = new Blackboard();
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
            m_BlackboardData?.SetValuesOnBlackboard(_blackboard);
            if (m_AutoInitialize)
                Initialize();
            else
                enabled = false;
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
            StateMachineController.nextState = StateMachineController.DefaultState;
            StateMachineController.CurrentState = StateMachineController.DefaultState;
            enabled = true;
        }

        public void EarlyUpdate()
        {
#if UNITY_EDITOR || DEBUG
            _startTime = Time.realtimeSinceStartup;
            LogState(CurrentStateName);
#endif

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
#if UNITY_EDITOR || DEBUG
            _frameTime = (Time.realtimeSinceStartup - _startTime) * 1000f; // In milliseconds
#endif
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

        public bool TryGet<T>(BlackboardKey key, out T value)
        {
            return _blackboard.TryGetValue(key, out value);
        }

        public T GetValue<T>(BlackboardKey key)
        {
            return _blackboard.GetValue<T>(key);
        }

        public BlackboardKey GetOrRegisterKey(string keyName)
        {
            return _blackboard.GetOrRegisterKey(keyName);
        }

        public void SetValue<T>(BlackboardKey key, T v)
        {
            _blackboard.SetValue(key, v);
        }

        public bool TryGet<T>(out T config, string key = "") where T : ScriptableObject
        {
            CacheConfig();
            var status = TryGet(typeof(T), out var result, key);
            config = (T)result;
            return status;
        }

        private bool TryGet(Type type, out object config, string key = "")
        {
            var configKey = $"{type.Name}{key}";
            if (!_configs.TryGetValue(configKey, out var configs))
            {
                config = null;
                return false;
            }

            if (configs.Count > 1)
                Debug.LogWarning($"There are more than one Config of type {type.Name} with key {key} is found under actor {name}");

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

#if UNITY_EDITOR || DEBUG

        public enum CornerPosition { TopLeft, TopRight, BottomLeft, BottomRight }
        [HideInInspector] public bool ShowStateLog = false;
        [HideInInspector] public CornerPosition LogPosition = CornerPosition.TopLeft;

        private string _previousStateName;
        private Rect _logWindowRect = new Rect(10, 10, 300, 100);
        private bool _isResizing = false;
        private Vector2 _resizeStartMousePosition;
        private Vector2 _resizeStartWindowSize;
        private float _startTime;
        private float _frameTime;
        private List<string> _states = new List<string>();
        private const int MaxStatesCount = 5;
        private const float ResizeHandleSize = 10f;

        private void OnGUI()
        {
            if (ShowStateLog)
            {
                switch (LogPosition)
                {
                    case CornerPosition.TopLeft:
                        _logWindowRect.position = new Vector2(10, 10);
                        break;
                    case CornerPosition.TopRight:
                        _logWindowRect.position = new Vector2(Screen.width - _logWindowRect.width - 10, 10);
                        break;
                    case CornerPosition.BottomLeft:
                        _logWindowRect.position = new Vector2(10, Screen.height - _logWindowRect.height - 10);
                        break;
                    case CornerPosition.BottomRight:
                        _logWindowRect.position = new Vector2(Screen.width - _logWindowRect.width - 10, Screen.height - _logWindowRect.height - 10);
                        break;
                }

                _logWindowRect = GUI.Window(0, _logWindowRect, DrawLogWindow, "Actor Log");
                ResizeWindowHandle();
            }
        }

        private void DrawLogWindow(int windowID)
        {
            GUIStyle logStyle = new GUIStyle
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            string logContent = string.Join("\n", _states.AsEnumerable().Reverse());
            GUILayout.Label(logContent, logStyle);
            GUILayout.Label("Frame Time: " + _frameTime.ToString("F4") + " ms", logStyle); // Display actor frame time

            GUI.DragWindow(new Rect(0, 0, _logWindowRect.width, 20));
        }

        private void ResizeWindowHandle()
        {
            Rect resizeHandleRect = new Rect(_logWindowRect.xMax - ResizeHandleSize, _logWindowRect.yMax - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
            GUI.DrawTexture(resizeHandleRect, Texture2D.whiteTexture);

            if (Event.current.type == EventType.MouseDown && resizeHandleRect.Contains(Event.current.mousePosition))
            {
                _isResizing = true;
                _resizeStartMousePosition = Event.current.mousePosition;
                _resizeStartWindowSize = _logWindowRect.size;
                Event.current.Use();
            }

            if (_isResizing)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    Vector2 mouseDelta = (Vector2)Event.current.mousePosition - _resizeStartMousePosition;
                    _logWindowRect.size = new Vector2(Mathf.Max(100, _resizeStartWindowSize.x + mouseDelta.x), Mathf.Max(50, _resizeStartWindowSize.y + mouseDelta.y));
                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseUp)
                {
                    _isResizing = false;
                    Event.current.Use();
                }
            }
        }

        public void LogState(string stateName)
        {
            if (_previousStateName != stateName)
            {
                _previousStateName = stateName;
                _states.Add(stateName);

                if (_states.Count > MaxStatesCount)
                    _states.RemoveAt(0);
            }
        }
#endif
    }
}