using System;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class Port
    {
        public Rect rect;
        public BaseNode node;
        public int id;
        public Port(BaseNode node, int id)
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

   /* public class BaseNode
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

        private System.Action<BaseNode> _removeNode;
        private System.Action<BaseNode> _startTransition;
        private System.Action<BaseNode> _setAsDefaultNode;
        private System.Action<BaseNode> _onMouseUp;
     
        public bool IsAnyStateNode => state.name.Equals(Util.AnyStateModelName);

        public BaseNode(StateModel state, Vector2 position, Action<BaseNode> startTransition, Action<BaseNode> makeTransition, Action<BaseNode> removeNode, Action<BaseNode> setAsDefaultNode)
        {
            this.state = state;
            stateModelSO = new SerializedObject(state);
            SetPosition(position);
            rect = new Rect(position.x, position.y, 200, 50);
            endPort = new Port(this, 1);
            startPort = new Port(this, 2);
           // normalNodeStyle = !IsAnyStateNode ? Settings.NodeNormalStyle() : Settings.AnyStateNodeStyle();
          //  selectedNodeStyle = !IsAnyStateNode ? Settings.NodeFocudeStyle() : Settings.AnyStateFocusedNodeStyle();
          //  defaultNormalNodeStyle = Settings.DefaultNodeStyle();
          //  defaultSelectedNodeStyle = Settings.DefaultFocusedNodeStyle();
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

        public Vector2 GetPosition()
        {
           return stateModelSO.FindProperty("position").vector3Value;
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
            if (!IsAnyStateNode)
                genericMenu.AddItem(new GUIContent("Set as Default State"), false, () => _setAsDefaultNode?.Invoke(this));
            if (!IsAnyStateNode)
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
    }*/
}
