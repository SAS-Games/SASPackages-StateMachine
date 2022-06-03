using System;
using UnityEditor;
using UnityEngine;
using SAS.Utilities.TagSystem.Editor;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;
using System.Linq;

namespace SAS.StateMachineGraph.Editor
{
    [CustomPropertyDrawer(typeof(KeyAttribute))]
    public class KeyDrawer : PropertyDrawer
    {
        private string[] Key => TagList.GetList(TagList.KeysIdentifier);
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var tag = property;
            var tagIndex = Array.IndexOf(Key, tag.stringValue);
            if (tagIndex != -1 || string.IsNullOrEmpty(tag.stringValue))
                EditorUtility.DropDown(0, position, Key, tagIndex, selectedIndex => SetTagSerializedProperty(tag, selectedIndex), AddKey);
            else
                EditorUtility.DropDown(0, position, Key, tagIndex, tag.stringValue, Color.red, selectedIndex => SetTagSerializedProperty(tag, selectedIndex), AddKey);

            EditorGUI.EndProperty();
        }

        private void AddKey()
        {
            var value = EditorInputDialog.Show("Add Key", "", "New Key");
            if (value == null)
                return;
            value = GetUniqueName(value, Key);
            TagList.Instance(TagList.KeysIdentifier).Add(value);
            UnityEditor.EditorUtility.SetDirty(TagList.Instance(TagList.KeysIdentifier));
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
            sp.stringValue = index != -1 ? Key[index] : string.Empty;
            sp.serializedObject.ApplyModifiedProperties();
        }
    }
}