using SAS.StateMachineGraph;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace SAS.StateMachineGraphEditor
{
    public class StateMachineModelOpenAsset
    {
        [OnOpenAssetAttribute(1)]
        public static bool step1(int instanceID, int line)
        {
            var stateMachineController = EditorUtility.InstanceIDToObject(instanceID);
            if(stateMachineController.GetType() == typeof(StateMachineModel))
                StateMachineEditorWindow.ShowBehaviourGraphEditor(stateMachineController as StateMachineModel);
            return false;
        }
    }
}
