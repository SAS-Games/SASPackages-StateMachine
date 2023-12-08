using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NotNullAttribute))]
public class NotNullDrawer : PropertyDrawer
{
    public override void OnGUI(Rect inRect, SerializedProperty inProp, GUIContent label)
    {
        EditorGUI.BeginProperty(inRect, label, inProp);

        bool error = inProp.objectReferenceValue == null;
        if (error)
        {
            label.text = "[!] " + label.text;
            GUI.color = Color.red;
            if (Application.isPlaying)
            {
                if (EditorUtility.DisplayDialog("Value Not Assigned Error", $"{inProp.propertyPath} is not assigned", "OK"))
                    EditorApplication.isPlaying = false;
            }
        }

        EditorGUI.PropertyField(inRect, inProp, label);
        GUI.color = Color.white;

        EditorGUI.EndProperty();
    }
}