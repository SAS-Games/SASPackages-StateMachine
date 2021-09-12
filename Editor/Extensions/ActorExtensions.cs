using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public static class ActorExtensions
    {
        public static void AddConfig(this Actor actor, ScriptableObject config, string key = "")
        {
            var actorSO = new SerializedObject(actor);
            var configsProp = actorSO.FindProperty("m_Configs");
            configsProp.InsertArrayElementAtIndex(configsProp.arraySize);
            var element = configsProp.GetArrayElementAtIndex(configsProp.arraySize - 1);
            var data = element.FindPropertyRelative("data");
            var name = element.FindPropertyRelative("name");
            data.objectReferenceValue = config;
            name.stringValue = key;
            actorSO.ApplyModifiedProperties();
        }
    }
}
