using UnityEngine;
using UnityEditor;
using System;

namespace SAS.StateMachineGraph.Editor
{
    public class Settings : ScriptableObject
    {
        [SerializeField] private Texture m_Arrow = default;
        [NonSerialized] private GUIStyle normalStyle;
        [NonSerialized] private GUIStyle focusedStyle;
        [NonSerialized] private GUIStyle defaultNormalStyle;
        [NonSerialized] private GUIStyle defaultFocusedStyle;

        public static implicit operator Settings(EditorSettings v)
        {
            throw new NotImplementedException();
        }

        private static Settings mInstance;

        public static Settings Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = Resources.Load("Settings", typeof(Settings)) as Settings;
                return mInstance;
            }
        }

        public static GUIStyle GetNodeStyle()
        {
            if (Instance.normalStyle == null)
            {
                Instance.normalStyle = new GUIStyle();
                Instance.normalStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
                Instance.normalStyle.border = new RectOffset(12, 12, 12, 12);
                Instance.normalStyle.alignment = TextAnchor.MiddleCenter;
            }
            return Instance.normalStyle;
         }

        public static GUIStyle GetNodeFocudeStyle()
        {
            if (Instance.focusedStyle == null)
            {
                Instance.focusedStyle = new GUIStyle();
                Instance.focusedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
                Instance.focusedStyle.border = new RectOffset(12, 12, 12, 12);
                Instance.focusedStyle.alignment = TextAnchor.MiddleCenter;
            }
            return Instance.focusedStyle;
        }

        public static GUIStyle GetDefaultNodeStyle()
        {
            if (Instance.defaultNormalStyle == null)
            {
                Instance.defaultNormalStyle = new GUIStyle();
                Instance.defaultNormalStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
                Instance.defaultNormalStyle.border = new RectOffset(12, 12, 12, 12);
                Instance.defaultNormalStyle.alignment = TextAnchor.MiddleCenter;
            }
            return Instance.defaultNormalStyle;
        }

        public static GUIStyle GetDefaultFocusedNodeStyle()
        {
            if (Instance.defaultFocusedStyle == null)
            {
                Instance.defaultFocusedStyle = new GUIStyle();
                Instance.defaultFocusedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5 on.png") as Texture2D;
                Instance.defaultFocusedStyle.border = new RectOffset(12, 12, 12, 12);
                Instance.defaultFocusedStyle.alignment = TextAnchor.MiddleCenter;
            }
            return Instance.defaultFocusedStyle;
        }

        

        public static Texture GetArrowTexture()
        {
            return Instance.m_Arrow;
        }
    }
}
