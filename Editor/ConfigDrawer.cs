using UnityEngine;
using UnityEditor;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;
using SAS.TagSystem.Editor;
using System;

namespace SAS.StateMachineGraph.Editor
{
   /* [CustomPropertyDrawer(typeof(Actor.Config))]
    public class ConfigDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position.width /= 2;
            EditorGUI.ObjectField(position, property.FindPropertyRelative("data"), new GUIContent());
            position.x = position.width + 20;
            var tag = property.FindPropertyRelative("name");
            EditorUtility.DropDown("ConfigDrawer".GetHashCode(), position, TaggerEditor.TagList, Array.IndexOf(TaggerEditor.TagList, tag.stringValue), selectedIndex => OnTagSelected(tag, selectedIndex));
            EditorGUI.EndProperty();
        }

        private void OnTagSelected(SerializedProperty serializedProperty, int index)
        {
            serializedProperty.stringValue = index != -1 ? TaggerEditor.TagList[index] : string.Empty;
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }
    }*/
}
