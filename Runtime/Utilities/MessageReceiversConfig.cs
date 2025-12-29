using SAS.Core.TagSystem;
using UnityEngine;

namespace SAS.StateMachineGraph.Utilities
{
    [CreateAssetMenu(menuName = "SAS/State Machine Character Controller/Message Receivers Config)")]
    public class MessageReceiversConfig : ScriptableObject
    {
        [SerializeField] private Tag[] m_MessageReceiversTags;
        [SerializeField] private string m_Message;
#if UNITY_EDITOR
        [SerializeField, TextArea] private string m_Description;
#endif

        public Tag[] MessageReceiversTags => m_MessageReceiversTags;
        public string Message => m_Message;
    }
}
