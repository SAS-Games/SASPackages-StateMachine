using UnityEngine;
using UnityEditor;
using System;

namespace SAS.StateMachineGraph.Editor
{
    public class Settings : ScriptableObject
    {
        [NonSerialized] private GUIStyle _childStateMachinestoolBarStyle;

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
    }
}
