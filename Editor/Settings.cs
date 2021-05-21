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
        [NonSerialized] private GUIStyle anyStateNormalStyle;
        [NonSerialized] private GUIStyle anyStateFocusedStyle;
        [NonSerialized] private GUIStyle _childStateMachinestoolBarStyle;
        [NonSerialized] private GUIStyle _stateMachineNormalStyle;
        [NonSerialized] private GUIStyle _stateMachineFocusedStyle;

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

        public static GUIStyle ChildStateMachinestoolBarStyle
        {
            get
            {
                if (Instance._childStateMachinestoolBarStyle == null)
                {
                    Instance._childStateMachinestoolBarStyle = new GUIStyle();
                    var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    texture.SetPixel(0, 0, new Color(0.1647059f, 0.1647059f, 0.1647059f));
                    texture.Apply();
                    Instance._childStateMachinestoolBarStyle.normal.background = texture;
                    Instance._childStateMachinestoolBarStyle.alignment = TextAnchor.MiddleCenter;
                    Instance._childStateMachinestoolBarStyle.normal.textColor = Color.cyan;
                    Instance._childStateMachinestoolBarStyle.fixedWidth = 120;
                    Instance._childStateMachinestoolBarStyle.margin = new RectOffset(1, 0, 0, 0);
                }
                return Instance._childStateMachinestoolBarStyle;
            }
        }

        public static GUIStyle NodeNormalStyle
        {
            get
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
        }

        public static GUIStyle NodeFocudeStyle
        {
            get
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
        }

        public static GUIStyle StateMachineNodeNormalStyle
        {
            get
            {
                if (Instance._stateMachineNormalStyle == null)
                    Instance._stateMachineNormalStyle = Array.Find(GUI.skin.customStyles, style => style.name.Equals("flow node hex 0"));
                return Instance._stateMachineNormalStyle;
            }
        }

        public static GUIStyle StateMachineNodeFocudeStyle
        {
            get
            {
                if (Instance._stateMachineFocusedStyle == null)
                {
                    Instance._stateMachineFocusedStyle = new GUIStyle();
                    Instance._stateMachineFocusedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node0 on.png") as Texture2D;
                    Instance._stateMachineFocusedStyle.border = new RectOffset(12, 12, 12, 12);
                    Instance._stateMachineFocusedStyle.alignment = TextAnchor.MiddleCenter;
                }
                return Instance._stateMachineFocusedStyle;
            }
        }

        public static GUIStyle DefaultNormalNodeStyle
        {
            get
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
        }

        public static GUIStyle DefaultFocusedNodeStyle
        {
            get
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
        }

        public static GUIStyle AnyStateNodeStyle
        {
            get
            {
                if (Instance.anyStateNormalStyle == null)
                {
                    Instance.anyStateNormalStyle = new GUIStyle();
                    Instance.anyStateNormalStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D;
                    Instance.anyStateNormalStyle.border = new RectOffset(12, 12, 12, 12);
                    Instance.anyStateNormalStyle.alignment = TextAnchor.MiddleCenter;
                }
                return Instance.anyStateNormalStyle;
            }
        }

        public static GUIStyle AnyStateFocusedNodeStyle
        {
            get
            {
                if (Instance.anyStateFocusedStyle == null)
                {
                    Instance.anyStateFocusedStyle = new GUIStyle();
                    Instance.anyStateFocusedStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2 on.png") as Texture2D;
                    Instance.anyStateFocusedStyle.border = new RectOffset(12, 12, 12, 12);
                    Instance.anyStateFocusedStyle.alignment = TextAnchor.MiddleCenter;
                }
                return Instance.anyStateFocusedStyle;
            }
        }



        public static Texture GetArrowTexture()
        {
            return Instance.m_Arrow;
        }
    }
}
