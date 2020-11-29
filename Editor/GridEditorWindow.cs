using UnityEngine;
using UnityEditor;

namespace SAS.StateMachineGraphEditor
{
    public class GridEditorWindow : EditorWindow
    {
        public Vector2 panOffset { get { return _panOffset; } set { _panOffset = value; Repaint(); } }
        private Vector2 _panOffset;
        public float zoom { get { return _zoom; } }// set { _zoom = Mathf.Clamp(value, NodeEditorPreferences.GetSettings().minZoom, NodeEditorPreferences.GetSettings().maxZoom); Repaint(); } }
        private float _zoom = 1;

        protected Vector2 offset;
        private Vector2 drag;

        protected virtual void OnEnable()
        {
        }
        protected virtual void OnGUI()
        {
            //DrawGrid(position, 1, offset);
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);
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
            }
        }

        protected virtual void OnDrag(Vector2 delta)
        {
            drag = delta;
        }

        public void DrawGrid(Rect rect, float zoom, Vector2 panOffset)
        {
            rect.position = Vector2.zero;
            Vector2 center = rect.size / 2f;
            Texture2D gridTex = null;//graphEditor.GetGridTexture(); 
            Texture2D crossTex = null;//graphEditor.GetSecondaryGridTexture();
            // Offset from origin in tile units            
            float xOffset = -(center.x * zoom + panOffset.x) / 64;//gridTex.width;
            float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / 64;// gridTex.height;
            Vector2 tileOffset = new Vector2(xOffset, yOffset);
            // Amount of tiles            
            float tileAmountX = Mathf.Round(rect.size.x * zoom) / 64;//gridTex.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / 64;//gridTex.height;
            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);
            // Draw tiled background            
            GUI.DrawTextureWithTexCoords(rect, gridTex, new Rect(tileOffset, tileAmount));
            GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);

            for (int j = 0; j < heightDivs; j++)
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        //====================================================================//
       /* [MenuItem("StateMachine/Editor")]
        public static GridEditorWindow Open()
        {
            GridEditorWindow w = GetWindow(typeof(GridEditorWindow), false, "StateMachineEditorWindow", true) as GridEditorWindow;
            w.wantsMouseMove = true;
            return w;
        }*/
    }
}
