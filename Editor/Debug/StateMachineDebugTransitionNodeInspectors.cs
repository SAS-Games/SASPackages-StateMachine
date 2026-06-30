using UnityEditor;

namespace SAS.StateMachineGraph.Editor
{
    [CustomEditor(typeof(EntryStateModel))]
    internal sealed class EntryStateModelDebugInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target))))
                DrawDefaultInspector();
            DrawDebugInspector();
        }

        private void DrawDebugInspector()
        {
            var controller = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target)) as RuntimeStateMachineController;
            StateMachineDebugInspector.DrawForNode((EntryStateModel)target, controller);
        }
    }

    [CustomEditor(typeof(ExitStateModel))]
    internal sealed class ExitStateModelDebugInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target))))
                DrawDefaultInspector();
            DrawDebugInspector();
        }

        private void DrawDebugInspector()
        {
            var controller = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(target)) as RuntimeStateMachineController;
            StateMachineDebugInspector.DrawForNode((ExitStateModel)target, controller);
        }
    }
}
