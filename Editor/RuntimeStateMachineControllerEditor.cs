using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(RuntimeStateMachineController))]
    public class RuntimeStateMachineControllerEditor : UnityEditor.Editor
    {
        private ReorderableList _tagsList;
        private ReorderableList _keysList;
        private bool showTagsList = false;
        private bool showKeysList = false;
        private List<string> _tags = new List<string>();
        private List<string> _keys = new List<string>();

        private void OnEnable()
        {
            var stateMachineModel = target as RuntimeStateMachineController;
            _tags.Clear();
            _tags.AddRange(stateMachineModel.tags);
            _tagsList = new ReorderableList(_tags, typeof(string), false, true, false, true);
            _tagsList.onRemoveCallback = (list) =>
            {
                List<string> tmp = new List<string>(stateMachineModel.tags);
                tmp.RemoveAt(list.index);
                stateMachineModel.tags = tmp.ToArray();
                _tags.RemoveAt(list.index);
                EditorUtility.SetDirty(target);
            };

            _keys.Clear();
            _keys.AddRange(stateMachineModel.tags);
            _keysList = new ReorderableList(stateMachineModel.keys, typeof(string), false, true, false, true);
            _keysList.onRemoveCallback = (list) =>
            {
                List<string> tmp = new List<string>(stateMachineModel.keys);
                tmp.RemoveAt(list.index);
                stateMachineModel.keys = tmp.ToArray();
                _keys.RemoveAt(list.index);
                EditorUtility.SetDirty(target);
            };
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            showTagsList = EditorGUILayout.Foldout(showTagsList, "Tags");
            if (showTagsList)
                _tagsList.DoLayoutList();

            showKeysList = EditorGUILayout.Foldout(showKeysList, "Keys");
            if (showKeysList)
                _keysList.DoLayoutList();

            if (GUILayout.Button("StateMachineGraphEditor"))
                StateMachineEditorWindow.ShowBehaviourGraphEditor(target as RuntimeStateMachineController);
        }
    }
}
