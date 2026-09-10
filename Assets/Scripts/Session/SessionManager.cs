using Core;
using UnityEngine;
using UnityEngine.InputSystem;
using VTT.Unity;

namespace Session
{
    public class SessionManager : MonoBehaviour
    {
        
        [SerializeField]
        private GridManager gridManager;
        
        private GameSession gameSession;

        public GameSession GetGameSession()
        {
            return gameSession;
        }

        public Room GetActiveRoom()
        {
            return gameSession?.ActiveRoom;
        }

        public void InitializeSession(string roomName, BoardGrid boardGrid, MapGenerator mapGenerator)
        {
            gameSession = new GameSession();

            Room firstRoom = new Room("room_1", roomName, boardGrid, mapGenerator);

            gameSession.AddRoom(firstRoom);

            Debug.Log($"[SESSION] Sessão criada. sala ativa: {roomName}");
        }

        public Room CreateRoom(string roomName, int width, int height)
        {
            if (gameSession == null)
            {
                Debug.LogError($"[SESSION] A sessão ainda não foi inicializada");

                return null;
            }

            MapGenerator generator = new MapGenerator(width, height);

            BoardGrid newBoard = generator.GenerateMap();

            string roomId = $"room_{gameSession.Rooms.Count + 1}";
            
            Room newRoom = new Room(roomId, roomName, newBoard, generator);
            
            gameSession.AddRoom(newRoom);
            
            Debug.Log($"[SESSION] Sala criada: {roomName} | ID: {roomId}");
            
            return newRoom;
        }
        
        public void SwitchRoom(Room room)
        {
            if (room == null)
                return;

            gameSession.SetActiveRoom(room);

            gridManager.LoadRoom(room);

            Debug.Log(
                $"[SESSION] Sala ativa alterada para: {room.Name}"
            );
        }
        
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
            {
                Room newRoom = CreateRoom(
                    "Sala Teste",
                    10,
                    10
                );

                SwitchRoom(newRoom);
            }
        }
    }
}
