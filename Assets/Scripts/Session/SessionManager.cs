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

        private int nextRoomId = 1;
        private int nextPlayerId = 1;

        public GameSession GetGameSession()
        {
            return gameSession;
        }

        public Room GetActiveRoom()
        {
            return gameSession?.ActiveRoom;
        }

        private void Start()
        {
            InitializeSession();
        }

        public void InitializeSession()
        {
            gameSession = new GameSession();

            nextRoomId = 1;
            nextPlayerId = 1;

            Room firstRoom =
                CreateRoom(
                    "Sala Inicial",
                    10,
                    10
                );
            
            SwitchRoom(firstRoom);

            Debug.Log(
                $"[SESSION] Sessão criada. Sala ativa: {firstRoom.Name}"
            );
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

            MapData mapData = generator.GetMapData();

            string roomId = $"room_{nextRoomId}";
            nextRoomId++;
            
            Room newRoom = new Room(roomId, roomName, newBoard, mapData);
            
            gameSession.AddRoom(newRoom);
            
            Debug.Log($"[SESSION] Sala criada: {roomName} | ID: {roomId}");
            
            return newRoom;
        }
        
        public void DeleteRoom(Room room)
        {
            if (gameSession == null)
            {
                Debug.LogError("[SESSION] A sessão ainda não foi inicializada.");
                return;
            }

            if (room == null)
                return;

            if (!gameSession.Rooms.Contains(room))
            {
                Debug.LogWarning("[SESSION] A sala não pertence a esta sessão.");
                return;
            }

            if (gameSession.Rooms.Count <= 1)
            {
                Debug.LogWarning("[SESSION] Não é possível remover a única sala da sessão.");
                return;
            }

            bool wasActiveRoom = gameSession.ActiveRoom == room;

            gameSession.RemoveRoom(room);

            Debug.Log($"[SESSION] Sala removida: {room.Name}");
            
            if (wasActiveRoom)
            {
                gridManager.LoadRoom(gameSession.ActiveRoom);

                Debug.Log(
                    $"[SESSION] Nova sala ativa: {gameSession.ActiveRoom.Name}"
                );
            }
        }
        
        public void SwitchRoom(Room room)
        {
            if (room == null)
                return;

            if (gameSession == null)
                return;

            gameSession.SetActiveRoom(room);

            gridManager.LoadRoom(room);

            Debug.Log(
                $"[SESSION] Sala ativa alterada para: {room.Name}"
            );
        }
        
        public SessionPlayer AddPlayer(string playerName)
        {
            if (gameSession == null)
            {
                Debug.LogError("[SESSION] A sessão ainda não foi inicializada.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(playerName))
            {
                Debug.LogWarning("[SESSION] Nome do jogador inválido.");
                return null;
            }

            string playerId = $"player_{nextPlayerId}";
            nextPlayerId++;

            SessionPlayer player =
                new SessionPlayer(playerId, playerName);

            gameSession.AddPlayer(player);

            Debug.Log(
                $"[SESSION] Jogador adicionado: {player.Name} | ID: {player.Id}"
            );

            return player;
        }
        
        public void RemovePlayer(SessionPlayer player)
        {
            if (gameSession == null)
                return;

            if (player == null)
                return;

            if (!gameSession.Players.Contains(player))
                return;

            gameSession.RemovePlayer(player);

            Debug.Log(
                $"[SESSION] Jogador removido: {player.Name} | ID: {player.Id}"
            );
        }
        
        public void AssignTokenToPlayer(
            SessionPlayer player,
            string tokenId)
        {
            if (gameSession == null)
                return;

            if (player == null)
                return;

            if (!gameSession.Players.Contains(player))
                return;

            if (string.IsNullOrEmpty(tokenId))
                return;

            player.AddControlledToken(tokenId);

            Debug.Log(
                $"[SESSION] Token {tokenId} atribuído ao jogador {player.Name}."
            );
        }
        
        public void RemoveTokenFromPlayer(
            SessionPlayer player,
            string tokenId)
        {
            if (gameSession == null)
                return;

            if (player == null)
                return;

            player.RemoveControlledToken(tokenId);

            Debug.Log(
                $"[SESSION] Token {tokenId} removido do jogador {player.Name}."
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
            
            if (Keyboard.current != null &&
                Keyboard.current.deleteKey.wasPressedThisFrame)
            {
                DeleteRoom(gameSession.ActiveRoom);
            }
        }
    }
}
