using System;
using System.Collections.Generic;

namespace SAS.StateMachineGraph.Editor
{
    public static class ReflectionHelpers
    {
        public static Type[] GetAllDerivedTypes<T>(this AppDomain appDomain) where T : class
        {
            var result = new List<Type>();
            var assemblies = appDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsAbstract && (type.IsSubclassOf(typeof(T)) || typeof(T).IsAssignableFrom(type)))
                        result.Add(type);
                }
            }
            return result.ToArray();
        }
    }
}