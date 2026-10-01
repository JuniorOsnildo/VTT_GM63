using System;
using Network.DTO;

namespace Network.Messages
{
    [Serializable]
    public class ActiveRoomChangedMessage
    {
        public NetworkMessageType Type;
        public RoomDTO Room;

        public ActiveRoomChangedMessage(RoomDTO room)
        {
            Type = NetworkMessageType.ActiveRoomChanged;
            Room = room;
        }
    }
}