using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class EditorUtilities
    {
        public static void VerticalLine(Rect rect, float width, Color color)
        {
            var style = new GUIStyle();
            style.normal.background = EditorGUIUtility.whiteTexture;
            style.fixedWidth = width;

            var c = GUI.color;
            GUI.color = color;
            GUI.Box(rect, GUIContent.none, style);
            GUI.color = c;
        }

        public static void HorizontalLine(Rect rect, float width, Color color)
        {
            var style = new GUIStyle();
            style.normal.background = EditorGUIUtility.whiteTexture;
            style.fixedHeight = width;

            var c = GUI.color;
            GUI.color = color;
            GUI.Box(rect, GUIContent.none, style);
            GUI.color = c;
        }
    }
}
