using System;

namespace Network.DTO
{
    [Serializable]
    public class SessionSnapshotDTO
    {
        public string ActiveRoomId;

        public RoomDTO ActiveRoom;

        public PlayerDTO[] Players;
    }
}