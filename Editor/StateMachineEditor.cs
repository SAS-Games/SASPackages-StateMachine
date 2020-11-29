using SAS.StateMachineGraph;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraphEditor
{
    [CustomEditor(typeof(StateMachineModel))]
    public class StateMachineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("StateMachineGraphEditor"))
                StateMachineEditorWindow.ShowBehaviourGraphEditor(target as StateMachineModel);
        }
    }
}
