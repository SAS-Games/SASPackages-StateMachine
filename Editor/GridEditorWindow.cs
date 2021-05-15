using UnityEngine;
using UnityEditor;

namespace SAS.StateMachineGraph.Editor
{
    public class GridEditorWindow : EditorWindow
    {
        public Vector2 panOffset { get { return _panOffset; } set { _panOffset = value; Repaint(); } }
        private Vector2 _panOffset;
        public float Zoom { get { return _zoom; } 
                            set { _zoom = Mathf.Clamp(value,0.2f, 2f); Repaint(); } }
        private float _zoom = 1f;

        protected Vector2 offset;
        private Vector2 drag;
        private Color _gridColor = Color.black;
        private Texture2D _texture;

        protected virtual void OnEnable()
        {
            _texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _texture.SetPixel(0, 0, new Color(0.1647059f, 0.1647059f, 0.1647059f));
            _texture.Apply();
        }
        protected virtual void OnGUI()
        {
            GUI.DrawTexture(new Rect(0, 0, position.xMax / Zoom, position.yMax / Zoom), _texture, ScaleMode.StretchToFill);
            DrawGrid(10, 0.25f, _gridColor);
            DrawGrid(100, 0.5f, _gridColor);
        }

        bool altKeyHeld;
        protected virtual void ProcessMouseEvent(Event e)
        {
            drag = Vector2.zero;

            switch (e.type)
            {
                case EventType.MouseDrag:
                    if (e.button == 0 && altKeyHeld)
                        OnDrag(e.delta);
                    break;
                case EventType.KeyDown:
                    if (e.keyCode == KeyCode.LeftAlt)
                        altKeyHeld = true;
                    break;
                case EventType.KeyUp:
                    if (e.keyCode == KeyCode.LeftAlt)
                        altKeyHeld = false;
                    break;
                case EventType.ScrollWheel:
                    Zoom += 0.025f * Mathf.Sign(e.delta.y);
                    break;
            }
        }

        protected virtual void OnDrag(Vector2 delta)
        {
            drag = delta;
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = (int)(Mathf.CeilToInt(position.width / gridSpacing) / Zoom);
            int heightDivs = (int)(Mathf.CeilToInt(position.height / gridSpacing) / Zoom);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height / Zoom, 0f) + newOffset);

            for (int j = 0; j < heightDivs; j++)
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width / Zoom, gridSpacing * j, 0f) + newOffset);

            Handles.color = Color.white;
            Handles.EndGUI();
        }
    }
}
