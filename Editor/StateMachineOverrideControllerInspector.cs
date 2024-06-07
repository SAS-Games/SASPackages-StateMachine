using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using EditorUtility = SAS.Utilities.Editor.EditorUtility;

namespace SAS.StateMachineGraph.Editor
{
    internal class StateActionOverrideComparer : IComparer<KeyValuePair<SerializedType, SerializedType>>
    {
        public int Compare(KeyValuePair<SerializedType, SerializedType> x, KeyValuePair<SerializedType, SerializedType> y)
        {
            return string.Compare(x.Key.GetType().AssemblyQualifiedName, y.Key.GetType().AssemblyQualifiedName, System.StringComparison.OrdinalIgnoreCase);
        }
    }

    [CustomEditor(typeof(StateMachineOverrideController))]
    [CanEditMultipleObjects]
    public class StateMachineOverrideControllerInspector : UnityEditor.Editor
    {
        SerializedProperty m_Controller;
       
        private List<KeyValuePair<string, string>> m_Actions;
        private ReorderableList m_ActionList;
        private string[] allUniqueAction;
        private Type[] _allActionTypes;

        internal int overridesCount
        {
            get
            {
                if (m_Controller == null)
                    return 0;
                if (allUniqueAction == null)
                {
                    var stateMachineOverrideController = m_Controller.hasMultipleDifferentValues ? null : (target as StateMachineOverrideController);
                    if (stateMachineOverrideController != null)
                    {
                        var runtimeStateMachineController = stateMachineOverrideController.runtimeStateMachineController;
                        if (runtimeStateMachineController != null)
                        {
                            allUniqueAction = runtimeStateMachineController.GetAllUniqueActions();
                            return allUniqueAction.Length;
                        }
                    }
                    allUniqueAction = new string[] { };
                
                }

                return allUniqueAction.Length;
            }
        }

        void OnEnable()
        {
            _allActionTypes = AppDomain.CurrentDomain.GetAllDerivedTypes<IStateAction>().ToArray();
            StateMachineOverrideController stateMachineOverrideController = target as StateMachineOverrideController;

            m_Controller = serializedObject.FindProperty("m_Controller");

            if (m_Actions == null)
                m_Actions = new List<KeyValuePair<string, string>>();

            if (m_ActionList == null)
            {
                GetOverrides(m_Actions);

                m_ActionList = new ReorderableList(m_Actions, typeof(KeyValuePair<string, string>), false, true, false, false);
                m_ActionList.drawElementCallback = DrawStateActionPairElement;
                m_ActionList.elementHeight = EditorGUIUtility.singleLineHeight * 3f;

                m_ActionList.drawHeaderCallback = DrawHeader;
            }
        }

        public override void OnInspectorGUI()
        {
            bool isEditingMultipleObjects = targets.Length > 1;
            bool changeCheck = false;
            serializedObject.UpdateIfRequiredOrScript();

            var stateMachineOverrideController = target as StateMachineOverrideController;
            var runtimeStateMachineController = m_Controller.hasMultipleDifferentValues ? null : stateMachineOverrideController.runtimeStateMachineController;
            EditorGUI.BeginChangeCheck();
            runtimeStateMachineController = EditorGUILayout.ObjectField("Controller", runtimeStateMachineController, typeof(RuntimeStateMachineController), false) as RuntimeStateMachineController;
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    var controller = targets[i] as StateMachineOverrideController;
                    controller.runtimeStateMachineController = runtimeStateMachineController;
                }

                changeCheck = true;
            }

            using (new EditorGUI.DisabledScope(m_Controller == null || (isEditingMultipleObjects && m_Controller.hasMultipleDifferentValues) || runtimeStateMachineController == null))
            {
                EditorGUI.BeginChangeCheck();
                GetOverrides(m_Actions);

                m_ActionList.list = m_Actions;
                m_ActionList.DoLayoutList();

                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        var controller = targets[i] as StateMachineOverrideController;
                        ApplyOverrides(controller, m_Actions);
                    }
                    changeCheck = true;
                }
            }

            if (changeCheck)
                serializedObject.ApplyModifiedProperties();
        }

        internal void GetOverrides(List<KeyValuePair<string, string>> overrides)
        {
            if (overrides == null)
                throw new System.ArgumentNullException("overrides");

            int count = overridesCount;
            if (overrides.Capacity < count)
                overrides.Capacity = count;

            overrides.Clear();
            for (int i = 0; i < count; ++i)
            {
                var originalAction = GetOriginalAction(i);
                overrides.Add(new KeyValuePair<string, string>(originalAction, GetOverrideAction(originalAction)));
            }
        }


        private string GetOriginalAction(int index)
        {
            return allUniqueAction[index];
        }

        private string GetOverrideAction(string originalAction)
        {
            var stateActionPairs = serializedObject.FindProperty("m_StateActionPairs");
            for (int i = 0; i < stateActionPairs.arraySize; ++i)
            {
                SerializedProperty serializedProperty = stateActionPairs.GetArrayElementAtIndex(i);
                var actionName = serializedProperty.FindPropertyRelative("original").stringValue;
                if (actionName.Equals(originalAction))
                    return stateActionPairs.GetArrayElementAtIndex(i).FindPropertyRelative("overridden").stringValue;
            }

            return null;
        }

        private void ApplyOverrides(StateMachineOverrideController stateMachineOverrideController, IList<KeyValuePair<string, string>> overrides)
        {
            if (overrides == null)
                throw new System.ArgumentNullException("overrides");
            var stateActionPairs = new SerializedObject(stateMachineOverrideController).FindProperty("m_StateActionPairs");
            stateActionPairs.ClearArray();
            for (int i = 0; i < overrides.Count; i++)
                SetAction(stateActionPairs, overrides[i].Key, overrides[i].Value);
        }

        private void SetAction(SerializedProperty stateActionPairs, string originalAction, string overrideAction)
        {
            Debug.Log(stateActionPairs.arraySize);
            var index = stateActionPairs.arraySize;
            stateActionPairs.InsertArrayElementAtIndex(index);
            SerializedProperty serializedProperty = stateActionPairs.GetArrayElementAtIndex(index);
            serializedProperty.FindPropertyRelative("original").stringValue = originalAction;
            serializedProperty.FindPropertyRelative("overridden").stringValue = overrideAction;
            serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Overridden Actions", EditorStyles.label);
        }

        private void DrawStateActionPairElement(Rect rect, int index, bool selected, bool focused)
        {
            rect.y -= 10;
            string originalAction = m_Actions[index].Key;
            string overrideAction = m_Actions[index].Value;
            var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
            EditorGUI.LabelField(rect, "Original:", style);
            rect.x += 70;
        
            EditorGUI.LabelField(rect, SerializedType.Sanitize(originalAction), new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft });
            rect.y += EditorGUIUtility.singleLineHeight;

            rect.x -= 70;
            EditorGUI.LabelField(rect, "Override:", style);
            rect.x += 70;
            rect.width -= 65;
            var curActionIndex = Array.FindIndex(_allActionTypes, ele => ele.AssemblyQualifiedName == overrideAction);
            rect.y += EditorGUIUtility.singleLineHeight;
            var pos = new Rect(rect.x, rect.y - 25, rect.width, rect.height);
            int id = GUIUtility.GetControlID("actionFullName".GetHashCode(), FocusType.Keyboard, rect);
            if (curActionIndex != -1 || string.IsNullOrEmpty(overrideAction))
                EditorUtility.DropDown(id, rect, pos, _allActionTypes.Select(ele => SerializedType.Sanitize(ele.ToString())).ToArray(), curActionIndex, selectedIndex => SetSelectedAction(index, selectedIndex));
            else
                EditorUtility.DropDown(id, rect, pos, _allActionTypes.Select(ele => SerializedType.Sanitize(ele.ToString())).ToArray(), curActionIndex, overrideAction, Color.red, selectedIndex => SetSelectedAction(index, selectedIndex));
           
        }

        private void SetSelectedAction(int elementIndex, int selectedIndex)
        {
            var overrideAction = "";
            if (selectedIndex == -1)
                overrideAction = null;
            else
                overrideAction = _allActionTypes[selectedIndex].AssemblyQualifiedName;

            m_Actions[elementIndex] = new KeyValuePair<string, string>(m_Actions[elementIndex].Key,overrideAction);
            ApplyOverrides(target as StateMachineOverrideController, m_Actions);
        }
    }
}
