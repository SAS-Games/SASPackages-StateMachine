using SAS.StateMachineGraph;
using UnityEngine;

namespace SAS.Utilities.DeveloperConsole
{
    [CreateAssetMenu(fileName = "New Actor Debug Command", menuName = "SAS/Utilities/DeveloperConsole/Commands/Actor Debug Command")]
    public class ActorDebugCommand : ConsoleCommand
    {
        public override string HelpText => $"Usage: {CommandWord} [true/false] [0/1/2/3]. Show or hide the OnScreen actor debug window at desire corner.";

        public override bool Process(string[] args, DeveloperConsoleBehaviour developerConsole)
        {
            if (args.Length == 0)
                return false;
            if (bool.TryParse(args[0], out var show))
            {
                var actor = FindFirstObjectByType<Actor>();
                if (actor != null)
                    actor.ShowStateLog = show;
                if (show)
                {
                    if (int.TryParse(args[1], out var pos))
                        actor.LogPosition = (Actor.CornerPosition)pos;

                }

                return true;
            }
            return false;
        }
    }
}
