using System;

namespace Network.Messages
{
    [Serializable]
    public class PlayerControlUpdatedMessage
    {
        public NetworkMessageType Type;
        public string PlayerId;
        public string[] ControlledTokenIds;

        public PlayerControlUpdatedMessage(string playerId, string[] controlledTokenIds)
        {
            Type = NetworkMessageType.PlayerControlUpdated;
            PlayerId = playerId;
            ControlledTokenIds = controlledTokenIds;
        }
    }
}