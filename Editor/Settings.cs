using UnityEngine;
using UnityEditor;
using System;

namespace SAS.StateMachineGraph.Editor
{
    public class Settings : ScriptableObject
    {
        [NonSerialized] private GUIStyle _childStateMachinestoolBarStyle;
        [NonSerialized] private Texture2D _darkGreyTexture2D;

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
                    Instance._childStateMachinestoolBarStyle = Array.Find(GUI.skin.customStyles, style => style.name.Equals("GUIEditor.BreadcrumbMidBackground"));
                    Instance._childStateMachinestoolBarStyle.alignment = TextAnchor.MiddleCenter;
                    Instance._childStateMachinestoolBarStyle.fixedWidth = 140;
                }
                return Instance._childStateMachinestoolBarStyle;
            }
        }

        public static Texture2D DarkGreyTexture
        {
            get
            {
                if (Instance._darkGreyTexture2D == null)
                {
                    Instance._darkGreyTexture2D = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    Instance._darkGreyTexture2D.SetPixel(0, 0, new Color(0.1647059f, 0.1647059f, 0.1647059f));
                    Instance._darkGreyTexture2D.Apply();
                }

                return Instance._darkGreyTexture2D;
            }
        }
    }
}
