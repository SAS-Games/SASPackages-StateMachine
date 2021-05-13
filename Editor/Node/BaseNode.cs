using System;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public abstract class BaseNode
    {
        public SerializedObject serializedObject;
        public Rect rect;
        private bool _isDragged;
        
        private GUIStyle _style;
        private GUIStyle _normalNodeStyle;
        private GUIStyle _selectedNodeStyle;

        public Port endPort;
        public Port startPort;

        protected abstract void ProcessContextMenu();
        protected abstract void ProcessMouseUp(BaseNode baseNode);
        protected virtual void ProcessOnDoubleClicked(BaseNode baseNode) { }
        private bool _isDoubleClicked = false;

        public BaseNode(SerializedObject serializedObject, Vector2 position, float width, float height, GUIStyle normal, GUIStyle selected)
        {
            this.serializedObject = serializedObject;
            Position = position;
            rect = new Rect(position.x, position.y, width, height);
            endPort = new Port(this, 1);
            startPort = new Port(this, 2);
            _normalNodeStyle = normal;
            _selectedNodeStyle = selected;
            _style = _normalNodeStyle;
        }

        public BaseNode(SerializedObject serializedObject, Vector2 position, GUIStyle normal, GUIStyle selected) : this(serializedObject, position, 180, 50, normal, selected) { }

        public void Draw()
        {
            endPort.Draw();
            startPort.Draw();
            GUI.Box(rect, serializedObject.targetObject.name, _style);
        }

        public virtual void Drag(Vector2 delta)
        {
            rect.position += delta;
            Position = rect.position;
        }

        public Vector3 Position
        {
            get { return serializedObject.FindProperty("m_Position").vector3Value; }
            private set
            {
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
                        StateModelInspector.NormalView();
                        _style = _selectedNodeStyle;
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
                        _style = _normalNodeStyle;

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
                            
                            ProcessMouseUp(this);
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
                        e.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }

        public bool IsFocused
        {
            get { return _style == _selectedNodeStyle; }
            set { _style = value ? _selectedNodeStyle : _normalNodeStyle; }

        }


    }
}
