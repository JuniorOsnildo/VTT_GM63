using System;

namespace Network.DTO
{
    [Serializable]
    public class TokenDTO
    {
        public string Id;
        public string Name;

        public int X;
        public int Y;

        public int Faction;

        public int MaxMovement;
        public int RemainingMovement;
    }
}