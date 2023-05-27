using SAS.Utilities.TagSystem;
using System;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    public static class ActorExtensions
    {
        public static bool TryGetComponent<T>(this Actor actor, out T component, Tag tag = Tag.None)
        {
            component = (T)(object)actor.GetComponent(typeof(T), tag);
            return component != null;
        }

        public static bool TryGetComponentInChildren<T>(this Actor actor, out T component, Tag tag = Tag.None, bool includeInactive = false)
        {
            component = (T)(object)actor.GetComponentInChildren(typeof(T), tag, includeInactive);
            return component != null;
        }

        public static Component GetComponent(this Actor actor, Type type, Tag tag = Tag.None)
        {
            var obj = TaggerExtensions.GetComponent(actor, type, tag);
            if (obj == null)
                Debug.LogError($"No component of type {type.Name} with tag {tag} is found under actor {actor.name}, attached on the game object {actor.gameObject.name}. Try assigning the component with the right Tag");

            return obj;
        }

        public static Component GetComponentInChildren(this Actor actor, Type type, Tag tag = Tag.None, bool includeInactive = false)
        {
            var obj = TaggerExtensions.GetComponentInChildren(actor, type, tag, includeInactive);
            if (obj == null)
                Debug.LogError($"No component of type {type.Name} with tag {tag} is found under actor {actor.name}, attached on the game object {actor.gameObject.name}. Try assigning the component with the right Tag");

            return obj;
        }

        public static bool TryGetComponentsInChildren<T>(this Actor actor, out T[] components, Tag tag, bool includeInactive = false)
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

        public static bool TryGetComponentsInParent<T>(this Actor actor, out T[] components, Tag tag = Tag.None, bool includeInactive = false)
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

        public static bool TryGetComponentInParent<T>(this Actor actor, out T component, Tag tag = Tag.None, bool includeInactive = false)
        {
            component = (T)(object)actor.GetComponentInParent(typeof(T), tag, includeInactive);
            return component != null;
        }

        public static Component GetComponentInParent(this Actor actor, Type type, Tag tag = Tag.None, bool includeInactive = false)
        {
            var obj = TaggerExtensions.GetComponentInParent(actor, type, tag, includeInactive);
            if (obj == null)
                Debug.LogError($"No component of type {type.GetType()} with tag {tag} is found under actor {actor.name}, attached on the game object {actor.gameObject.name}. Try assigning the component with the right Tag");

            return obj;
        }

        public static T Get<T>(this Actor actor, string key = "") where T : ScriptableObject
        {
            if (actor.TryGet<T>(out var service, key))
                return service;

            return default(T);
        }
    }
}
