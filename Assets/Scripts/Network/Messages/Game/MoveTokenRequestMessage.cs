using System;

namespace Network.Messages
{
    [Serializable]
    public class MoveTokenRequestMessage
    {
        public NetworkMessageType Type;

        public string PlayerId;
        public string RoomId;
        public string TokenId;

        public int TargetX;
        public int TargetY;

        public MoveTokenRequestMessage(
            string playerId,
            string roomId,
            string tokenId,
            int targetX,
            int targetY)
        {
            Type = NetworkMessageType.MoveTokenRequest;

            PlayerId = playerId;
            RoomId = roomId;
            TokenId = tokenId;

            TargetX = targetX;
            TargetY = targetY;
        }
    }
}