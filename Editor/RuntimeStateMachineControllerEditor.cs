using SAS.TagSystem.Editor;
using System.Collections.Generic;
using System.Linq;
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
            _tags.AddRange(TagList.Instance().values);
            _tagsList = new ReorderableList(_tags, typeof(string), false, true, false, true);
            _tagsList.onRemoveCallback = (list) =>
            {
                TagList.Instance().Remove(list.index);
                _tags.RemoveAt(list.index);
                EditorUtility.SetDirty(TagList.Instance());
            };

            _keys.Clear();
            _keys.AddRange(TagList.Instance(TagList.KeysIdentifier).values);
            _keysList = new ReorderableList(_keys, typeof(string), false, true, false, true);
            _keysList.onRemoveCallback = (list) =>
            {
                TagList.Instance(TagList.KeysIdentifier).Remove(list.index);
                _keys.RemoveAt(list.index);
                EditorUtility.SetDirty(TagList.Instance(TagList.KeysIdentifier));                
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

            if (GUILayout.Button("Fix Tags and Keys"))
            {
                var runtimeStateMachineController = target as RuntimeStateMachineController;
                var stateModels = runtimeStateMachineController.GetAllStateModels();
                var usedTags = new List<string>();
                var usedKeys = new List<string>();
                foreach (var stateModel in stateModels)
                {
                    usedTags.AddRange(stateModel.GetUsedTags());
                    usedKeys.AddRange(stateModel.GetUsedKeys());
                }

                usedTags = usedTags.Distinct().ToList();
                usedKeys = usedKeys.Distinct().ToList();
                TagList.Instance().AddRange(usedTags);
                TagList.Instance(TagList.KeysIdentifier).AddRange(usedKeys);
            }
        }
    }
}
