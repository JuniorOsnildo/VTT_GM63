using System;

namespace Network.Messages
{
    [Serializable]
    public class JoinRequestMessage
    {
        public NetworkMessageType Type;

        public string PlayerId;
        public string PlayerName;

        public JoinRequestMessage(
            string playerId,
            string playerName)
        {
            Type = NetworkMessageType.JoinRequest;

            PlayerId = playerId;
            PlayerName = playerName;
        }
    }
}