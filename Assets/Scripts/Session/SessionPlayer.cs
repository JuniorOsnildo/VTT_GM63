using System.Collections.Generic;

namespace Session
{
    public class SessionPlayer
    {
        public string Id { get; private set; }
        public bool IsConnected { get; private set; }
        public string Name { get; private set; }

        public List<string> ControlledTokenIds { get; private set; }

        public SessionPlayer(string id, string name)
        {
            Id = id;
            Name = name;
            
            IsConnected = true;
            
            ControlledTokenIds = new List<string>();
        }

        public void AddControlledToken(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
                return;

            if (ControlledTokenIds.Contains(tokenId))
                return;

            ControlledTokenIds.Add(tokenId);
        }

        public void RemoveControlledToken(string tokenId)
        {
            ControlledTokenIds.Remove(tokenId);
        }

        public bool ControlsToken(string tokenId)
        {
            return ControlledTokenIds.Contains(tokenId);
        }
        
        public void Connect()
        {
            IsConnected = true;
        }

        public void Disconnect()
        {
            IsConnected = false;
        }
    }
}