using System;
using Network.DTO;

namespace Network.Messages
{
    [Serializable]
    public class TokenCreatedMessage
    {
        public NetworkMessageType Type;
        public string RoomId;
        public TokenDTO Token;

        public TokenCreatedMessage(
            string roomId,
            TokenDTO token)
        {
            Type = NetworkMessageType.TokenCreated;
            RoomId = roomId;
            Token = token;
        }
    }
}