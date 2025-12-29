using SAS.Core.TagSystem;
using System.Collections.Generic;
using UnityEngine;


namespace SAS.StateMachineGraph.Utilities
{
    public class SendMessageUpwards : IStateAction
    {
        private MessageReceiversConfig _messageReceiversConfig;
        private List<Actor> _messageReceivers = new List<Actor>();
        private Actor _actor;

        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            _actor = actor;
            actor.TryGet(out _messageReceiversConfig, key);
            foreach (var messageReceiverTag in _messageReceiversConfig.MessageReceiversTags)
            {
                actor.TryGetComponentsInParent(out Actor[] messageReceivers, messageReceiverTag);
                _messageReceivers.AddRange(messageReceivers);
            }
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            foreach (var messageReceiver in _messageReceivers)
                messageReceiver.SendMessage(_messageReceiversConfig.Message, _actor,
                    SendMessageOptions.DontRequireReceiver);
        }
    }
}