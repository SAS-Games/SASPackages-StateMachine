using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using EditorUtility = SAS.Core.Editor.EditorUtility;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(StateMachineOverrideController))]
    public class StateMachineOverrideControllerInspector : UnityEditor.Editor
    {
        private SerializedProperty m_Controller;
        private SerializedProperty m_ActionPairs;
        private SerializedProperty m_StatePairs;
        private ReorderableList _stateList;
        private StateModel[] _runtimeStates;


        private RuntimeStateMachineController _runtime;
        private Type[] _allActionTypes;

        private ReorderableList _actionList;

        private void OnEnable()
        {
            m_Controller = serializedObject.FindProperty("m_Controller");
            m_ActionPairs = serializedObject.FindProperty("m_ActionOverrides");
            m_StatePairs = serializedObject.FindProperty("m_StateOverrides");

            _allActionTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IStateAction).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();

            BuildActionList();
            BuildStateList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawControllerField();
            GUILayout.Space(10);

            DrawActionOverridesSection();
            GUILayout.Space(14);
            DrawStateOverridesSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawControllerField()
        {
            EditorGUI.BeginChangeCheck();
            var controller = EditorGUILayout.ObjectField("Controller", m_Controller.objectReferenceValue,
                typeof(RuntimeStateMachineController), false);

            if (EditorGUI.EndChangeCheck())
                m_Controller.objectReferenceValue = controller;

            _runtime = controller as RuntimeStateMachineController;
        }

        private void DrawActionOverridesSection()
        {
            EditorGUILayout.LabelField("Action Overrides", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(_runtime == null))
            {
                _actionList.DoLayoutList();
            }
        }

        private void BuildActionList()
        {
            _actionList = new ReorderableList(serializedObject, m_ActionPairs, draggable: false, displayHeader: false,
                displayAddButton: true, displayRemoveButton: true);
            _actionList.elementHeight = (EditorGUIUtility.singleLineHeight * 2) + 14;

            _actionList.onAddCallback = list =>
            {
                int index = list.serializedProperty.arraySize;
                list.serializedProperty.InsertArrayElementAtIndex(index);

                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("original").stringValue = null;
                element.FindPropertyRelative("overridden").stringValue = null;
            };

            _actionList.drawElementCallback = DrawActionElement;
        }

        private void DrawActionElement(Rect rect, int index, bool active, bool focused)
        {
            var element = m_ActionPairs.GetArrayElementAtIndex(index);
            var original = element.FindPropertyRelative("original");
            var overridden = element.FindPropertyRelative("overridden");

            rect.y += 4;
            rect.height = EditorGUIUtility.singleLineHeight;

            const float labelWidth = 120f;

            var labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            var fieldRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height);

            EditorGUI.LabelField(labelRect, "Original Action");

            string[] originalOptions = _runtime != null ? _runtime.GetAllUniqueActions() : Array.Empty<string>();
            string[] originalDisplay = originalOptions.Select(SerializedType.Sanitize).ToArray();
            int originalIndex = Array.IndexOf(originalOptions, original.stringValue);
            int originalId = GUIUtility.GetControlID(FocusType.Keyboard);

            EditorUtility.DropDown(originalId, fieldRect, fieldRect, originalOptions, originalDisplay, originalIndex,
                "Select Action", Color.white, selected =>
                {
                    original.stringValue =
                        selected < 0 ? null : originalOptions[selected];

                    overridden.stringValue = null;
                    serializedObject.ApplyModifiedProperties();
                }
            );

            rect.y += EditorGUIUtility.singleLineHeight + 6;

            labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            fieldRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height);

            EditorGUI.LabelField(labelRect, "Override Action");

            string[] overrideOptions = _allActionTypes.Select(t => t.AssemblyQualifiedName).ToArray();
            string[] overrideDisplay = _allActionTypes.Select(t => SerializedType.Sanitize(t.FullName)).ToArray();

            int overrideIndex = Array.FindIndex(overrideOptions, x => x == overridden.stringValue);
            int overrideId = GUIUtility.GetControlID(FocusType.Keyboard);

            EditorUtility.DropDown(overrideId, fieldRect, fieldRect, overrideOptions, overrideDisplay, overrideIndex,
                "None", Color.white, selected =>
                {
                    overridden.stringValue =
                        selected < 0 ? null : overrideOptions[selected];

                    serializedObject.ApplyModifiedProperties();
                }
            );
        }

        private void DrawStateOverridesSection()
        {
            EditorGUILayout.LabelField("State Overrides", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(_runtime == null))
            {
                _stateList.DoLayoutList();
            }
        }

        private void BuildStateList()
        {
            _stateList = new ReorderableList(serializedObject, m_StatePairs, draggable: false, displayHeader: false,
                displayAddButton: true, displayRemoveButton: true);

            _stateList.elementHeight =
                (EditorGUIUtility.singleLineHeight * 2) + 14;

            _stateList.onAddCallback = list =>
            {
                int index = list.serializedProperty.arraySize;
                list.serializedProperty.InsertArrayElementAtIndex(index);

                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("original").objectReferenceValue = null;
                element.FindPropertyRelative("overridden").objectReferenceValue = null;
            };

            _stateList.drawElementCallback = DrawStateElement;
        }

        private void DrawStateElement(Rect rect, int index, bool active, bool focused)
        {
            var element = m_StatePairs.GetArrayElementAtIndex(index);
            var original = element.FindPropertyRelative("original");
            var overridden = element.FindPropertyRelative("overridden");

            rect.y += 4;
            rect.height = EditorGUIUtility.singleLineHeight;

            const float labelWidth = 120f;

            _runtimeStates = _runtime != null ? _runtime.GetAllStateModels().ToArray() : Array.Empty<StateModel>();

            var labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            var fieldRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height);

            EditorGUI.LabelField(labelRect, "Original State");

            string[] stateNames = _runtimeStates.Select(s => s.name).ToArray();
            int selectedIndex = Array.IndexOf(_runtimeStates, original.objectReferenceValue);

            int id = GUIUtility.GetControlID(FocusType.Keyboard);

            EditorUtility.DropDown(id, fieldRect, fieldRect, stateNames, stateNames, selectedIndex, "Select State",
                Color.white, selected =>
                {
                    original.objectReferenceValue =
                        selected < 0 ? null : _runtimeStates[selected];
                    overridden.objectReferenceValue = null;
                    serializedObject.ApplyModifiedProperties();
                }
            );

            rect.y += EditorGUIUtility.singleLineHeight + 6;

            labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            fieldRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height);

            EditorGUI.LabelField(labelRect, "Override State");

            EditorGUI.BeginChangeCheck();
            var newOverride = EditorGUI.ObjectField(
                fieldRect,
                overridden.objectReferenceValue,
                typeof(StateModel),
                false);

            if (EditorGUI.EndChangeCheck())
            {
                overridden.objectReferenceValue = newOverride;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}