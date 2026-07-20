using UnityEditor;
using UnityEditor.Callbacks;

namespace SAS.StateMachineGraph.Editor
{
    public class StateMachineModelOpenAsset
    {
        [OnOpenAsset(1)]
        public static bool OpenStateMachineGraph(int instanceID, int line)
        {
            var stateMachineController = EditorUtility.EntityIdToObject(instanceID);
            if(stateMachineController.GetType() == typeof(RuntimeStateMachineController))
                StateMachineEditorWindow.ShowBehaviourGraphEditor(stateMachineController as RuntimeStateMachineController);
            return false;
        }
    }
}
