using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public static class Util
    {
        public const string AnyStateModelName = "Any State";
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

        public static Vector2Int ToVector2Int(this Vector2 vector2)
        {
            return new Vector2Int((int)Mathf.RoundToInt(vector2.x), (int)Mathf.RoundToInt(vector2.y));
        }

        public static Rect ToRect(this RectInt rectInt)
        {
            return new Rect(rectInt.x, rectInt.y, rectInt.width, rectInt.height);
        }
    }
}
