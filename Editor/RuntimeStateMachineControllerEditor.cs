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
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

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
