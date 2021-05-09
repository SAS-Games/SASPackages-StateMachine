using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(RuntimeStateMachineController))]
    public class StateMachineEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("StateMachineGraphEditor"))
                StateMachineEditorWindow.ShowBehaviourGraphEditor(target as RuntimeStateMachineController);
        }
    }
}
