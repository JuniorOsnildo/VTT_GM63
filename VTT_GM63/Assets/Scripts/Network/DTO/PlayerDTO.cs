using System;

namespace Network.DTO
{
    [Serializable]
    public class PlayerDTO
    {
        public string Id;
        public string Name;

        public bool IsConnected;

        public string[] ControlledTokenIds;
    }
} 