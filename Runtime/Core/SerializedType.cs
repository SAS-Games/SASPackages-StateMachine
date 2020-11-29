using System;
using UnityEngine;

namespace SAS.StateMachineGraph
{
    [Serializable]
    public class SerializedType
#if UNITY_EDITOR
        : ISerializationCallbackReceiver
#endif
    {
        public string fullName;

        public Type ToType()
        {
            return Type.GetType(fullName);
        }

        public override string ToString()
        {
            return Sanitize(fullName);
        }

        public static string Sanitize(string typeAsString)
        {
            if (typeAsString.Contains(","))
                typeAsString = typeAsString.Split(',')[0];
            return typeAsString;
        }

        public void OnBeforeSerialize()
        {
           // throw new NotImplementedException();
        }

        public void OnAfterDeserialize()
        {
           // throw new NotImplementedException();
        }
    }
}
