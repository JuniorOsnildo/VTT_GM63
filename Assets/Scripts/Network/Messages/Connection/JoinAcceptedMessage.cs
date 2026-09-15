using System;

namespace Network.Messages
{
    [Serializable]
    public class JoinAcceptedMessage
    {
        public NetworkMessageType Type;

        public string PlayerId;

        public JoinAcceptedMessage(string playerId)
        {
            Type = NetworkMessageType.JoinAccepted;
            PlayerId = playerId;
        }
    }
}