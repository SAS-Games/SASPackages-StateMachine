using System.Collections.Generic;

namespace SAS.StateMachineGraphEditor
{
    public static class Util
    {
        public static string MakeUniqueName(string nameBase, HashSet<string> usedNames)
        {
            string name = nameBase;
            int counter = 1;
            while (usedNames.Contains(name.Trim()))
            {
                name = nameBase + " " + counter;
                counter++;
            }
            usedNames.Add(name);
            return name;
        }
    }
}
