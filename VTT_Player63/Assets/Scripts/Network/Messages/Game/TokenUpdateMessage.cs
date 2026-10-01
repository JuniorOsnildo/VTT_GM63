using System;

namespace Network.Messages
{
    [Serializable]
    public class TokenUpdatedMessage
    {
        public NetworkMessageType Type;

        public string RoomId;
        public string TokenId;

        public string Name;
        public int MaxMovement;

        public TokenUpdatedMessage(string roomId, string tokenId, string name, int maxMovement)
        {
            Type = NetworkMessageType.TokenUpdated;

            RoomId = roomId;
            TokenId = tokenId;

            Name = name;
            MaxMovement = maxMovement;
        }
    }
}