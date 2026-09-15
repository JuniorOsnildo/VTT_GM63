using System;

namespace Network.Messages
{
    [Serializable]
    public class TokenMovedMessage
    {
        public NetworkMessageType Type;

        public string RoomId;
        public string TokenId;

        public int X;
        public int Y;

        public int RemainingMovement;

        public TokenMovedMessage(
            string roomId,
            string tokenId,
            int x,
            int y,
            int remainingMovement)
        {
            Type = NetworkMessageType.TokenMoved;

            RoomId = roomId;
            TokenId = tokenId;

            X = x;
            Y = y;

            RemainingMovement = remainingMovement;
        }
    }
}