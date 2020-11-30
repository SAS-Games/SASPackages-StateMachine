using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(StateMachineModel))]
    public class StateMachineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("StateMachineGraphEditor"))
                StateMachineEditorWindow.ShowBehaviourGraphEditor(target as StateMachineModel);
        }
    }
}
