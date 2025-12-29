using SAS.Core.TagSystem;
using System.Collections.Generic;
using UnityEngine;


namespace SAS.StateMachineGraph.Utilities
{
    public class SendMessage : IStateAction
    {
        private MessageReceiversConfig _messageReceiversConfig;
        private List<Actor> _messageReceivers = new List<Actor>();
        private Actor _actor;


        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            _actor = actor;
            actor.TryGet(out _messageReceiversConfig, key);
            foreach (var messageReceiversTag in _messageReceiversConfig.MessageReceiversTags)
            {
                actor.TryGetComponentsInChildren(out Actor[] messageReceivers, messageReceiversTag);
                _messageReceivers.AddRange(messageReceivers);
            }
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            foreach (var messageReceiver in _messageReceivers)
                messageReceiver.SendMessage(_messageReceiversConfig.Message, _actor, SendMessageOptions.DontRequireReceiver);
        }
    }
}
