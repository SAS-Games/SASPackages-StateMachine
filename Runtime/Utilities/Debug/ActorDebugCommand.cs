using SAS.StateMachineGraph;
using UnityEngine;

namespace SAS.Utilities.DeveloperConsole
{
    [CreateAssetMenu(fileName = "New Actor Debug Command", menuName = "SAS/Utilities/DeveloperConsole/Commands/Actor Debug Command")]
    public class ActorDebugCommand : ConsoleCommand
    {
        public override string HelpText => $"Usage: {Name} [true/false] [0/1/2/3]. Show or hide the OnScreen actor debug window at desire corner.";

        public override bool Process(DeveloperConsoleBehaviour developerConsole, string command, string[] args)
        {
#if DEBUG
            string showStateLog = "false";
            var actor = FindFirstObjectByType<Actor>();
            if (actor)
                showStateLog = actor.ShowStateLog ? "false" : "true";
            if (args == null || args.Length == 0)
                args = new string[] { showStateLog, "1" };

            if (bool.TryParse(args[0], out var show))
            {

                if (actor != null)
                    actor.ShowStateLog = show;
                if (show)
                {
                    if (int.TryParse(args[1], out var pos))
                        actor.LogPosition = (Actor.CornerPosition)pos;

                }

                return true;
            }
#endif
            return false;
        }
    }
}
