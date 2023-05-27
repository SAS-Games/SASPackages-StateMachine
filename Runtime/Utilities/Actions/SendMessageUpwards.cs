using SAS.Utilities.TagSystem;
using System.Collections.Generic;
using UnityEngine;


namespace SAS.StateMachineGraph.Utilities
{
    public class SendMessageUpwards : IStateAction
    {
        private MessageRecieversConfig _messageRecieversConfig;
        private List<Actor> _messageRecievers = new List<Actor>();
        private Actor _actor;
        void IStateAction.OnInitialize(Actor actor, Tag tag, string key)
        {
            _actor = actor;
            actor.TryGet(out _messageRecieversConfig, key);
            foreach (var messageRecieverTag in _messageRecieversConfig.MessageRecieverTags)
            {
                actor.TryGetComponentsInParent(out Actor[] messageRecievers, messageRecieverTag);
                _messageRecievers.AddRange(messageRecievers);
            }
        }

        void IStateAction.Execute(ActionExecuteEvent executeEvent)
        {
            foreach (var messageReciever in _messageRecievers)
                messageReciever.SendMessage(_messageRecieversConfig.Message, _actor, SendMessageOptions.DontRequireReceiver);
        }
    }
}
