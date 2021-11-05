using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities.Editor
{
    [CustomPropertyDrawer(typeof(Parameter))]
    public class ParameterDrawer : PropertyDrawer
    {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var height = EditorGUIUtility.singleLineHeight;

			EditorGUI.BeginProperty(position, label, property);
			label.text = "Parameter";

			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			var fieldPosition = new Rect(position.x, position.y + 5, position.width, height);
			SerializedProperty animParamName = property.FindPropertyRelative("m_Name");
			EditorGUI.PropertyField(fieldPosition, animParamName, new GUIContent("Name"));


			fieldPosition = new Rect(position.x, position.y + height + 5, position.width, height);
			SerializedProperty animParamValue = property.FindPropertyRelative("m_Type");
			EditorGUI.PropertyField(fieldPosition, animParamValue, new GUIContent("Type"));

			fieldPosition = new Rect(position.x, position.y + height * 2 + 5, position.width, height);

			switch (animParamValue.intValue)
			{
				case (int)ParameterType.Bool:
					EditorGUI.PropertyField(fieldPosition, property.FindPropertyRelative("m_BoolValue"), new GUIContent("Value"));
					break;
				case (int)ParameterType.Int:
					EditorGUI.PropertyField(fieldPosition, property.FindPropertyRelative("m_IntValue"), new GUIContent("Value"));
					break;
				case (int)ParameterType.Float:
					EditorGUI.PropertyField(fieldPosition, property.FindPropertyRelative("m_FloatValue"), new GUIContent("Value"));
					break;
				case (int)ParameterType.Trigger:
					EditorGUI.PropertyField(fieldPosition, property.FindPropertyRelative("m_BoolValue"), new GUIContent("Reset"));
					break;
			}

			EditorGUI.EndProperty();
		}

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
			return base.GetPropertyHeight(property, label) * 3 + 5;
        }
    }
}
