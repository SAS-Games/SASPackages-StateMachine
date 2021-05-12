using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace SAS.StateMachineGraph.Editor
{
    public class StateMachineControllerNameModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            var runtimeStateMachineController = AssetDatabase.LoadMainAssetAtPath(sourcePath) as RuntimeStateMachineController;
            if (runtimeStateMachineController == null)
                return AssetMoveResult.DidNotMove;
            
            var sourceDir = Path.GetDirectoryName(sourcePath);
            var destinationPathDir = Path.GetDirectoryName(destinationPath);
            if (sourcePath != destinationPath)
                return AssetMoveResult.DidNotMove;

            var fileName = Path.GetFileNameWithoutExtension(destinationPath);
            runtimeStateMachineController.name = fileName;


            return AssetMoveResult.DidNotMove;
        }
    }
}
