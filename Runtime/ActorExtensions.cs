using SAS.TagSystem;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public static class ActorExtensions
    {
        public static bool TryGetComponent<T>(this Actor actor, out T component, string tag = "")
        {
            component = (T)(object)actor.GetComponent(typeof(T), tag);
            return component != null;
        }

        public static bool TryGetComponentInChildren<T>(this Actor actor, out T component, string tag = "", bool includeInactive = false)
        {
            component = (T)(object)actor.GetComponentInChildren(typeof(T), tag, includeInactive);
            return component != null;
        }

        public static Component GetComponent(this Actor actor, Type type, string tag = "")
        {
            var obj = TaggerExtensions.GetComponent(actor, type, tag);
            if (obj == null)
                Debug.LogError($"No component of type {type.Name} with tag {tag} is found under actor {actor.name}, attached on the game object {actor.gameObject.name}. Try assigning the component with the right Tag");

            return obj;
        }

        public static Component GetComponentInChildren(this Actor actor, Type type, string tag = "", bool includeInactive = false)
        {
            var obj = TaggerExtensions.GetComponentInChildren(actor, type, tag, includeInactive);
            if (obj == null)
                Debug.LogError($"No component of type {type.Name} with tag {tag} is found under actor {actor.name}, attached on the game object {actor.gameObject.name}. Try assigning the component with the right Tag");

            return obj;
        }

        public static bool TryGetComponentsInChildren<T>(this Actor actor, out T[] components, string tag, bool includeInactive = false)
        {
            var results = actor.GetComponentsInChildren(typeof(T), tag, includeInactive);
            try
            {
                if (results.Length == 0)
                    results = null;

                components = new T[results.Length];
                for (int i = 0; i < results.Length; ++i)
                    components[i] = (T)(object)results[i];
            }
            catch (Exception)
            {
                components = null;
                Debug.LogError($"No component of type {typeof(T)} with tag {tag} is found under actor {actor.name}, attached on the game object {actor.gameObject.name}. Try assigning the component with the right Tag");
                return false;
            }

            return true;
        }

        public static bool TryGetComponentsInParent<T>(this Actor actor, out T[] components, string tag = "", bool includeInactive = false)
        {
            var results = actor.GetComponentsInParent(typeof(T), tag, includeInactive);
            try
            {
                if (results.Length == 0)
                    results = null;

                components = new T[results.Length];
                for (int i = 0; i < results.Length; ++i)
                    components[i] = (T)(object)results[i];
            }
            catch (Exception)
            {
                components = null;
                Debug.LogError($"No component of type {components.GetType()} with tag {tag} is found in parent of actor {actor.name}, attached on the game object {actor.gameObject.name}. Try assigning the component with the right Tag");
                return false;
            }

            return true;
        }

        public static bool TryGetComponentInParent<T>(this Actor actor, out T component, string tag = "", bool includeInactive = false)
        {
            component = (T)(object)actor.GetComponentInParent(typeof(T), tag, includeInactive);
            return component != null;
        }

        public static Component GetComponentInParent(this Actor actor, Type type, string tag = "", bool includeInactive = false)
        {
            var obj = TaggerExtensions.GetComponentInParent(actor, type, tag, includeInactive);
            if (obj == null)
                Debug.LogError($"No component of type {type.GetType()} with tag {tag} is found under actor {actor.name}, attached on the game object {actor.gameObject.name}. Try assigning the component with the right Tag");

            return obj;
        }

        public static T Get<T>(this Actor actor, string tag = "")
        {
            if (actor.TryGet<T>(out var service, tag))
                return service;

            return default(T);
        }

        public static IEnumerable<T> GetAll<T>(this Actor actor, string tag = "")
        {
            return ComponentExtensions.serviceLocator.GetAll<T>(tag);
        }

        public static void Add(this Actor actor, Type type, object service, string tag = "")
        {
            ComponentExtensions.serviceLocator.Add(type, service, tag);
        }

        public static void Add<T>(this Actor actor, object service, string tag = "")
        {
            actor.Add(typeof(T), service, tag);
        }

        public static T GetOrCreate<T>(this Actor actor, string tag = "")
        {
            return ComponentExtensions.serviceLocator.GetOrCreate<T>(tag);
        }

        public static bool Remove<T>(this Actor actor, string tag = "")
        {
            return ComponentExtensions.serviceLocator.Remove<T>(tag);
        }
    }
}
