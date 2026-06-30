using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SAS.StateMachineGraph.Editor
{
    public abstract class BaseNode
    {
        protected string _normalStyleName = "flow node 0";
        protected string _focusedStyleName = "flow node 0 on";
        internal Object TargetObject { get; }
        internal RectInt rect;
        private bool _isDragged;

        private GUIStyle _normalStyle;
        private GUIStyle _focusedStyle;

        public Port endPort;
        public Port startPort;
        private string _debugActorName = string.Empty;
        private string _debugGraphName = string.Empty;

        protected abstract void ProcessContextMenu();
        protected abstract void ProcessMouseUp(BaseNode baseNode, Event e);
        protected virtual void ProcessOnDoubleClicked(BaseNode baseNode) { }
        private bool _isDoubleClicked = false;
        protected string Prefix { get; set; }

        public BaseNode(Object targetObject, Vector2Int position, int width, int height)
        {
            TargetObject = targetObject;
            rect = new RectInt(position.x, position.y, width, height);
            endPort = new Port(this, 1);
            startPort = new Port(this, 2);
        }

        public BaseNode(Object targetObject, Vector2Int position) : this(targetObject, position, 180, 40) { }

        public void Draw()
        {
            endPort.Draw();
            startPort.Draw();
            GUI.Box(rect.ToRect(), $"{Prefix} {TargetObject?.name}", Style);
        }

        internal void SetDebugContext(Actor actor, string graphName)
        {
            _debugActorName = StateMachineDebugNodeKey.GetActorName(actor);
            if (string.IsNullOrEmpty(_debugActorName))
                _debugActorName = StateMachineDebugSession.Instance.CurrentActorName;

            _debugGraphName = graphName;
            if (string.IsNullOrEmpty(_debugGraphName))
                _debugGraphName = StateMachineDebugSession.Instance.CurrentGraphName;
        }

        protected void AddDebugBreakpointMenu(GenericMenu genericMenu)
        {
            if (this is ParentStateMachineNode)
                return;

            var nodeKey = StateMachineDebugNodeKey.Create(_debugActorName, _debugGraphName, TargetObject);
            if (string.IsNullOrEmpty(nodeKey))
            {
                genericMenu.AddDisabledItem(new GUIContent("Breakpoints/State Enter"));
                genericMenu.AddDisabledItem(new GUIContent("Breakpoints/State Exit"));
                return;
            }

            var session = StateMachineDebugSession.Instance;
            var hasEnterBreakpoint = session.HasBreakpoint(nodeKey, StateMachineDebugEventType.NodeEnter);
            var hasExitBreakpoint = session.HasBreakpoint(nodeKey, StateMachineDebugEventType.NodeExit);
            genericMenu.AddItem(
                new GUIContent("Breakpoints/State Enter"),
                hasEnterBreakpoint,
                () => session.ToggleBreakpoint(nodeKey, StateMachineDebugEventType.NodeEnter));
            genericMenu.AddItem(
                new GUIContent("Breakpoints/State Exit"),
                hasExitBreakpoint,
                () => session.ToggleBreakpoint(nodeKey, StateMachineDebugEventType.NodeExit));
        }

        private void ProcessDebugContextMenu()
        {
            if (this is ParentStateMachineNode)
                return;

            GenericMenu genericMenu = new GenericMenu();
            AddDebugBreakpointMenu(genericMenu);
            genericMenu.ShowAsContext();
        }

        public virtual void Drag(Vector2Int delta)
        {
            rect.position += delta;
            Position = (Vector3Int)rect.position;
        }

        public virtual Vector3Int Position
        {
            get { return new SerializedObject(TargetObject).FindProperty("m_Position").vector3IntValue; }
            protected set
            {
                var serializedObject = new SerializedObject(TargetObject);
                serializedObject.FindProperty("m_Position").vector3IntValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public bool ProcessEvents(Event e, bool isReadOnlyMode)
        {
            switch (e.type)
            {
                case EventType.MouseDown:

                    if (rect.Contains(e.mousePosition.ToVector2Int()))
                    {
                        e.Use();
                        IsFocused = true;
                        if (e.button == 0)
                        {
                            _isDoubleClicked = e.clickCount == 2;
                            _isDragged = !_isDoubleClicked;
                        }
                        if (e.button == 1)
                        {
                            _isDragged = false;
                            e.Use();
                        }
                        return true;
                    }
                    else
                        IsFocused = false;

                    break;

                case EventType.MouseUp:
                    _isDragged = false;
                    if (rect.Contains(e.mousePosition.ToVector2Int()))
                    {
                        if (e.button == 0)
                        {
                            if (_isDoubleClicked)
                            {
                                ProcessOnDoubleClicked(this);
                                return false;
                            }

                            ProcessMouseUp(this, e);
                            return true;
                        }
                        else if (e.button == 1)
                        {
                            if (isReadOnlyMode)
                            {
                                ProcessDebugContextMenu();
                                e.Use();
                                return false;
                            }
                            ProcessContextMenu();
                            e.Use();
                        }
                    }
                    _isDoubleClicked = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && _isDragged && !isReadOnlyMode)
                    {
                        Drag(e.delta.ToVector2Int());
                        return true;
                    }
                    break;
            }

            return false;
        }

        public bool IsFocused
        {
            get;
            set;
        }

        public GUIStyle Style
        {
            get
            {
                return IsFocused ? FocusedStyle : NormalStyle;
            }
        }

        public GUIStyle NormalStyle
        {
            get
            {
                if (_normalStyle == null || _normalStyle.name != _normalStyleName)
                    _normalStyle = Array.Find(GUI.skin.customStyles, style => style.name.Equals(_normalStyleName));
                return _normalStyle;
            }
        }

        public GUIStyle FocusedStyle
        {
            get
            {
                if (_focusedStyle == null || _focusedStyle.name != _normalStyleName)
                    _focusedStyle = Array.Find(GUI.skin.customStyles, style => style.name.Equals(_focusedStyleName));
                return _focusedStyle;
            }
        }
    }
}
