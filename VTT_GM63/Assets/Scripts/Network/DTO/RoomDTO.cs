using System;

namespace Network.DTO
{
    [Serializable]
    public class RoomDTO
    {
        public string Id;
        public string Name;

        public MapDTO Map;

        public TokenDTO[] Tokens;
    }
}