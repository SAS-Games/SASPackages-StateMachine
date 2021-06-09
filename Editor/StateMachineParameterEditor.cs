using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace SAS.StateMachineGraph.Editor
{
    public class StateMachineParameterEditor
    {
        public HashSet<string> UsedParameterNames
        {
            get
            {
                var parameters = _parametersList.serializedProperty.serializedObject?.FindProperty("_parameters");
                HashSet<string> usedParameterNames = new HashSet<string>();
                for (int i = 0; i < parameters.arraySize; ++i)
                {
                    var element = parameters.GetArrayElementAtIndex(i);
                    var name = element.FindPropertyRelative("m_Name");
                    usedParameterNames.Add(name.stringValue);
                }
                return usedParameterNames;
            }
        }

        private ReorderableList _parametersList;
        public Rect rect;
        private Texture2D _texture;

        public StateMachineParameterEditor(SerializedObject serializedObject)
        {
            _texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _texture.SetPixel(0, 0, new Color(0.2196079f, 0.2196079f, 0.2196079f));
            _texture.Apply();

            var parameters = serializedObject?.FindProperty("_parameters");
            _parametersList = new ReorderableList(serializedObject, parameters, true, true, false, true);

            for (int i = 0; i < parameters.arraySize; ++i)
            {
                var element = parameters.GetArrayElementAtIndex(i);
                var name = element.FindPropertyRelative("m_Name");
                UsedParameterNames.Add(name.stringValue);
            }

            HandleParameterList("Parameters");
        }

        public void DrawRect(Rect rect)
        {
            this.rect = rect;

            GUI.DrawTexture(new Rect(0, 0, rect.xMax, rect.yMax), Settings.GreyTexture, ScaleMode.StretchToFill);
        }

        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:

                    if (rect.Contains(e.mousePosition))
                    {
                        if (e.button == 0)
                            GUI.changed = true;
                        if (e.button == 1)
                        {
                            ProcessContextMenu();
                            e.Use();
                        }
                    }
                    break;
            }

            return false;
        }

        public void DrawParametersWindow(int windowId)
        {
            ProcessEvents(Event.current);
            _parametersList.DoList(rect);
            _parametersList?.serializedProperty?.serializedObject?.ApplyModifiedProperties();
        }

        private void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Float"), false, () => AddParameter(1));
            genericMenu.AddItem(new GUIContent("Int"), false, () => AddParameter(3));
            genericMenu.AddItem(new GUIContent("Bool"), false, () => AddParameter(4));
            genericMenu.AddItem(new GUIContent("Trigger"), false, () => AddParameter(9));
            genericMenu.ShowAsContext();
        }

        private void HandleParameterList(string name)
        {
            _parametersList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, name);
            };

            _parametersList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _parametersList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                SerializedProperty type = element.FindPropertyRelative("m_Type");
                SerializedProperty parameterName = element.FindPropertyRelative("m_Name");
                string paramName = EditorGUI.DelayedTextField(new Rect(rect.x, rect.y, rect.width / 1.5f, rect.height), parameterName.stringValue);
                if (parameterName.stringValue != paramName)
                    parameterName.stringValue = Util.MakeUniqueName(paramName, UsedParameterNames);

                if (type.intValue == 1)
                {
                    SerializedProperty value = element.FindPropertyRelative("m_DefaultFloat");
                    value.floatValue = EditorGUI.FloatField(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width - 10 - rect.width / 1.5f, rect.height), value.floatValue);
                }

                if (type.intValue == 3)
                {
                    SerializedProperty value = element.FindPropertyRelative("m_DefaultInt");
                    value.intValue = EditorGUI.IntField(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width - 10 - rect.width / 1.5f, rect.height), value.intValue);
                }

                if (type.intValue == 4)
                {
                    SerializedProperty value = element.FindPropertyRelative("m_DefaultBool");
                    value.boolValue = EditorGUI.Toggle(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width - 10 - rect.width / 1.5f, rect.height), value.boolValue);
                }

                if (type.intValue == 9)
                {
                    SerializedProperty value = element.FindPropertyRelative("m_DefaultBool");
                    value.boolValue = EditorGUI.Toggle(new Rect(rect.x + rect.width / 1.5f + 10, rect.y, rect.width - 10 - rect.width / 1.5f, rect.height), value.boolValue, EditorStyles.radioButton);
                }
            };
        }

        private void AddParameter(int type)
        {
            _parametersList.serializedProperty.InsertArrayElementAtIndex(_parametersList.serializedProperty.arraySize);
            var element = _parametersList.serializedProperty.GetArrayElementAtIndex(_parametersList.serializedProperty.arraySize - 1);

            SerializedProperty property = element.FindPropertyRelative("m_Type");
            SerializedProperty name = element.FindPropertyRelative("m_Name");
            property.intValue = type;
            if (type == 1)
                name.stringValue = Util.MakeUniqueName("New Float", UsedParameterNames);
            if (type == 3)
                name.stringValue = Util.MakeUniqueName("New Int", UsedParameterNames);
            if (type == 4)
                name.stringValue = Util.MakeUniqueName("New Bool", UsedParameterNames);
            if (type == 9)
                name.stringValue = Util.MakeUniqueName("New Trigger", UsedParameterNames);

            var defaultFloat = element.FindPropertyRelative("m_DefaultFloat");
            var defaultInt = element.FindPropertyRelative("m_DefaultInt");
            var defaultBool = element.FindPropertyRelative("m_DefaultBool");

            defaultFloat.floatValue = 0;
            defaultInt.intValue = 0;
            defaultBool.boolValue = false;
        }
    }
}
