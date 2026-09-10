using System.Collections.Generic;

namespace Session
{
    public class GameSession
    {
        public List<Room> Rooms { get; private set; }

        public Room ActiveRoom { get; private set; }

        public GameSession()
        {
            Rooms = new List<Room>();
        }

        public void AddRoom(Room room)
        {
            if (room == null)
                return;

            Rooms.Add(room);

            // Se for a primeira sala criada,
            // ela vira automaticamente a sala ativa.
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

            // Se apagamos a sala ativa,
            // escolhe outra sala, caso exista.
            if (ActiveRoom == room)
            {
                ActiveRoom =
                    Rooms.Count > 0
                        ? Rooms[0]
                        : null;
            }
        }
    }
}