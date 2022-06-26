using System.Collections.Generic;
using UnityEngine;


namespace SAS.StateMachineGraph.Utilities
{
    public class SendMessage : IStateAction
    {
        private MessageRecieversConfig _messageRecieversConfig;
        private List<Actor> _messageRecievers = new List<Actor>();
        private Actor _actor;


        void IStateAction.OnInitialize(Actor actor, string tag, string key, State state)
        {
            _actor = actor;
            actor.TryGet(out _messageRecieversConfig, key);
            foreach (var messageRecieverTag in _messageRecieversConfig.MessageRecieverTags)
            {
                actor.TryGetComponentsInChildren(out Actor[] messageRecievers, messageRecieverTag);
                _messageRecievers.AddRange(messageRecievers);
            }
        }

        void IStateAction.Execute()
        {
            foreach (var messageReciever in _messageRecievers)
                messageReciever.SendMessage(_messageRecieversConfig.Message, _actor, SendMessageOptions.DontRequireReceiver);
        }
    }
}
