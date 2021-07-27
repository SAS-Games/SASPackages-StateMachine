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
        internal Rect rect;
        private bool _isDragged;

        private GUIStyle _normalStyle;
        private GUIStyle _focusedStyle;

        public Port endPort;
        public Port startPort;

        protected abstract void ProcessContextMenu();
        protected abstract void ProcessMouseUp(BaseNode baseNode, Event e);
        protected virtual void ProcessOnDoubleClicked(BaseNode baseNode) { }
        private bool _isDoubleClicked = false;
        protected string Prefix { get; set; }

        public BaseNode(Object targetObject, Vector2 position, float width, float height)
        {
            TargetObject = targetObject;
            Position = position;
            rect = new Rect(position.x, position.y, width, height);
            endPort = new Port(this, 1);
            startPort = new Port(this, 2);
        }

        public BaseNode(Object targetObject, Vector2 position) : this(targetObject, position, 180, 40) { }

        public void Draw()
        {
            endPort.Draw();
            startPort.Draw();
            GUI.Box(rect, $"{Prefix} {TargetObject?.name}", Style);
        }

        public virtual void Drag(Vector2 delta)
        {
            rect.position += delta;
            Position = rect.position;
        }

        public virtual Vector3 Position
        {
            get { return new SerializedObject(TargetObject).FindProperty("m_Position").vector3Value; }
            protected set
            {
                var serializedObject = new SerializedObject(TargetObject);
                serializedObject.FindProperty("m_Position").vector3Value = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:

                    if (rect.Contains(e.mousePosition))
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
                    if (rect.Contains(e.mousePosition))
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
                            ProcessContextMenu();
                            e.Use();
                        }
                    }
                    _isDoubleClicked = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && _isDragged)
                    {
                        Drag(e.delta);
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
