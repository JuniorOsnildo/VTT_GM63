using System.Collections.Generic;

namespace Session
{
    public class GameSession
    {
        public List<Room> Rooms { get; private set; }
        public List<SessionPlayer> Players { get; private set; }
        public Room ActiveRoom { get; private set; }

        public GameSession()
        {
            Rooms = new List<Room>();
            Players = new List<SessionPlayer>();
        }

        public void AddRoom(Room room)
        {
            if (room == null)
                return;

            Rooms.Add(room);
            
            ActiveRoom ??= room;
        }

        public void SetActiveRoom(Room room)
        {
            if (room == null) 
                return;

            if (!Rooms.Contains(room))
                return;

            ActiveRoom = room;
        }

        public void RemoveRoom(Room room)
        {
            if (room == null)
                return;

            if (!Rooms.Contains(room))
                return;

            Rooms.Remove(room);
            
            if (ActiveRoom == room)
            {
                ActiveRoom =
                    Rooms.Count > 0
                        ? Rooms[0]
                        : null;
            }
        }
        
        public void AddPlayer(SessionPlayer player)
        {
            if (player == null)
                return;

            if (Players.Contains(player))
                return;

            Players.Add(player);
        }

        public void RemovePlayer(SessionPlayer player)
        {
            if (player == null)
                return;

            Players.Remove(player);
        }
        
        public SessionPlayer GetPlayerById(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                return null;

            return Players.Find(player => player.Id == playerId);
        }
    }
}