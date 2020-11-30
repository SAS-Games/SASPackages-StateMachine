using System;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class Port
    {
        public Rect rect;
        public Node node;
        public int id;
        public Port(Node node, int id)
        {
            this.node = node;
            this.id = id;
            rect = new Rect(0, 0, 2f, 2f);
        }

        public void Draw()
        {
            rect.height = node.rect.height * 0.5f;
            rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;
            switch (id)
            {
                case 1:
                    rect.x = node.rect.x + node.rect.width / 2 - 10;
                    break;

                case 2:
                    rect.x = node.rect.x + node.rect.width / 2 + 10;
                    break;
            }
        }
    }

    public class Node
    {
        public StateModel state;
        public SerializedObject stateModelSO;
        public Rect rect;
        public bool isDragged;
        public bool isDefault;
        private GUIStyle style;
        private GUIStyle defaultSelectedNodeStyle;
        private GUIStyle defaultNormalNodeStyle;
        private GUIStyle normalNodeStyle;
        private GUIStyle selectedNodeStyle;

        public Port endPort;
        public Port startPort;

        private System.Action<Node> _removeNode;
        private System.Action<Node> _startTransition;
        private System.Action<Node> _setAsDefaultNode;
        private System.Action<Node> _onMouseUp;
        private bool _swapped = false;
        public void SwapPort(bool swap)
        {
            if (swap && !_swapped)
            {
                var temp = startPort.id;
                startPort.id = endPort.id;
                endPort.id = temp;
            }
            else
                _swapped = true;
        }

        public Node(StateModel state, Vector2 position, Action<Node> startTransition, Action<Node> makeTransition, Action<Node> removeNode, Action<Node> setAsDefaultNode)
        {
            this.state = state;
            stateModelSO = new SerializedObject(state);
            SetPosition(position);
            rect = new Rect(position.x, position.y, 200, 50);
            endPort = new Port(this, 1);
            startPort = new Port(this, 2);
            normalNodeStyle = Settings.GetNodeStyle();
            selectedNodeStyle = Settings.GetNodeFocudeStyle();
            defaultNormalNodeStyle = Settings.GetDefaultNodeStyle();
            defaultSelectedNodeStyle = Settings.GetDefaultFocusedNodeStyle();
            SetStyle(false);
            _removeNode = removeNode;
            _startTransition = startTransition;
            _setAsDefaultNode = setAsDefaultNode;
            _onMouseUp = makeTransition;
        }

        public void Drag(Vector2 delta)
        {
            rect.position += delta;
            SetPosition(rect.position);
        }

        private void SetPosition(Vector3 position)
        {
            stateModelSO.FindProperty("position").vector3Value = position;
            stateModelSO.ApplyModifiedProperties();
        }

        public void Draw()
        {
            endPort.Draw();
            startPort.Draw();
            GUI.Box(rect, stateModelSO.targetObject.name, style);
        }

        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:

                    if (rect.Contains(e.mousePosition))
                    {
                        StateModelInspector.NormalView();
                        SetStyle(true);

                        if (e.button == 0)
                            isDragged = true;
                        if (e.button == 1)
                        {
                            isDragged = false;
                            e.Use();
                        }
                        return true;
                    }
                    else
                        SetStyle(false);
                    
                    break;

                case EventType.MouseUp:
                    isDragged = false;
                    if (rect.Contains(e.mousePosition))
                    {
                        if (e.button == 0)
                        {
                            _onMouseUp?.Invoke(this);
                            return true;
                        }
                        else if (e.button == 1)
                        {
                            ProcessContextMenu();
                            e.Use();
                        }
                    }
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged)
                    {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }

        private void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Make Transition"), false, () => _startTransition?.Invoke(this));
            genericMenu.AddItem(new GUIContent("Set as Default State"), false, () => _setAsDefaultNode?.Invoke(this));
            genericMenu.AddItem(new GUIContent("Delete"), false, () => _removeNode?.Invoke(this));
            genericMenu.ShowAsContext();
        }

        private void SetStyle(bool isSelected)
        {
            if (isSelected)
                style = isDefault ? defaultSelectedNodeStyle : selectedNodeStyle;
            else
                style = isDefault ? defaultNormalNodeStyle : normalNodeStyle;
        }

        public void SetActvieStyle(bool isActive)
        {
            SetStyle(isActive);
        }
    }
}
