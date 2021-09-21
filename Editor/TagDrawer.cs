using System;
using UnityEditor;
using UnityEngine;
using SAS.TagSystem.Editor;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;
using System.Linq;

namespace SAS.StateMachineGraph.Editor
{
    [CustomPropertyDrawer(typeof(TagAttribute))]
    public class TagDrawer : PropertyDrawer
    {
        private string[] Tags => TagList.GetList();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var tag = property;
            var tagIndex = Array.IndexOf(Tags, tag.stringValue);
            if (tagIndex != -1 || string.IsNullOrEmpty(tag.stringValue))
                EditorUtility.DropDown(0, position, Tags, tagIndex, selectedIndex => SetTagSerializedProperty(tag, selectedIndex), AddTag);
            else
                EditorUtility.DropDown(0, position, Tags, tagIndex, tag.stringValue, Color.red, selectedIndex => SetTagSerializedProperty(tag, selectedIndex), AddTag);

            EditorGUI.EndProperty();
        }

        private void AddTag()
        {
            var value = EditorInputDialog.Show("Add Tag", "", "New Tag");
            if (value == null)
                return;
            value = GetUniqueName(value, Tags);
            TagList.Instance().Add(value);
        }

        private string GetUniqueName(string nameBase, string[] usedNames)
        {
            string name = nameBase;
            int counter = 1;
            while (usedNames.Contains(name.Trim()))
            {
                name = nameBase + " " + counter;
                counter++;
            }
            return name;
        }

        private void SetTagSerializedProperty(SerializedProperty sp, int index)
        {
            sp.stringValue = index != -1 ? Tags[index] : string.Empty;
            sp.serializedObject.ApplyModifiedProperties();
        }
    }
}