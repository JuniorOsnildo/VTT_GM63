using System;
using System.Collections.Generic;
using Core;

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

        public PathCoordinate[] Path;

        public TokenMovedMessage(
            string roomId,
            string tokenId,
            int x,
            int y,
            List<GridCoordinate> path)
        {
            Type = NetworkMessageType.TokenMoved;

            RoomId = roomId;
            TokenId = tokenId;

            X = x;
            Y = y;

            if (path == null)
            {
                Path = Array.Empty<PathCoordinate>();
                return;
            }

            Path = new PathCoordinate[path.Count];

            for (int i = 0; i < path.Count; i++)
            {
                Path[i] = new PathCoordinate(
                    path[i].X,
                    path[i].Y
                );
            }
        }
    }

    [Serializable]
    public class PathCoordinate
    {
        public int X;
        public int Y;

        public PathCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}