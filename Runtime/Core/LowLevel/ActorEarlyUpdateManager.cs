using System.Collections.Generic;
namespace SAS.StateMachineGraph
{
    public static class ActorEarlyUpdateManager
    {
        static readonly List<Actor> actors = new();

        public static void Register(Actor actor) => actors.Add(actor);
        public static void Unregister(Actor actor) => actors.Remove(actor);

        public static void ActorEarlyUpdates()
        {
            if (actors.Count == 0) return;

            foreach (var actor in actors)
            {
                if (actor._isActiveAndEnabled)
                    actor.EarlyUpdate();
            }
        }

        public static void Clear()
        {
            actors.Clear();
        }
    }
}