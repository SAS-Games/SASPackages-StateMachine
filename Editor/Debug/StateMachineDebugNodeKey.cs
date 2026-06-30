using Object = UnityEngine.Object;

namespace SAS.StateMachineGraph.Editor
{
    internal static class StateMachineDebugNodeKey
    {
        internal static string Create(string actorName, string graphName, string nodeName)
        {
            if (string.IsNullOrEmpty(actorName) ||
                string.IsNullOrEmpty(graphName) ||
                string.IsNullOrEmpty(nodeName))
                return string.Empty;

            return $"{actorName}/{graphName}/{nodeName}";
        }

        internal static string Create(Actor actor, string graphName, ITransitionNode node)
        {
            return Create(GetActorName(actor), graphName, GetNodeName(node));
        }

        internal static string Create(string actorName, string graphName, Object nodeObject)
        {
            return Create(actorName, graphName, GetNodeName(nodeObject));
        }

        internal static string GetActorName(Actor actor)
        {
            return actor != null ? actor.name : string.Empty;
        }

        internal static string GetNodeName(ITransitionNode node)
        {
            return node != null ? node.Name : string.Empty;
        }

        internal static string GetNodeName(Object nodeObject)
        {
            return nodeObject != null ? nodeObject.name : string.Empty;
        }
    }
}
