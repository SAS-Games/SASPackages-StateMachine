using SAS.Utilities.TagSystem.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

            if (GUILayout.Button("Fix Keys"))
            {
                var runtimeStateMachineController = target as RuntimeStateMachineController;
                var stateModels = runtimeStateMachineController.GetAllStateModels();
                var usedKeys = new List<string>();
                foreach (var stateModel in stateModels)
                {
                    usedKeys.AddRange(stateModel.GetUsedKeys());
                }

                usedKeys = usedKeys.Distinct().ToList();
                KeyList.Instance().AddRange(usedKeys);
            }
        }
    }
}
