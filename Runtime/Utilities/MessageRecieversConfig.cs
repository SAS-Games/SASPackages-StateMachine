using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    [CreateAssetMenu(menuName = "SAS/State Machine Character Controller/Message Recievers Config)")]
    public class MessageRecieversConfig : ScriptableObject
    {
        [SerializeField, Tag] private string[] m_MessageRecieverTags;
        [SerializeField] private string m_Message;
#if UNITY_EDITOR
        [SerializeField, TextArea] private string m_Description;
#endif

        public string[] MessageRecieverTags => m_MessageRecieverTags;
        public string Message => m_Message;
    }
}
