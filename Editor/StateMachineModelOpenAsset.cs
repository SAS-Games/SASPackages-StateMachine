using UnityEditor;
using UnityEditor.Callbacks;

namespace SAS.StateMachineGraph.Editor
{
    public class StateMachineModelOpenAsset
    {
        [OnOpenAsset(1)]
        public static bool OpenStateMachineGraph(int instanceID, int line)
        {
#if UNITY_6000_3_OR_NEWER
            var stateMachineController = EditorUtility.EntityIdToObject(instanceID);
#else
         var stateMachineController = EditorUtility.InstanceIDToObject(instanceID);
#endif
            if(stateMachineController.GetType() == typeof(RuntimeStateMachineController))
                StateMachineEditorWindow.ShowBehaviourGraphEditor(stateMachineController as RuntimeStateMachineController);
            return false;
        }
    }
}
