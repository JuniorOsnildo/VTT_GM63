using Core;
using Network.DTO;
using Network.Messages;
using Network.Serialization;
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
        
        public SessionPlayer AddPlayer(
            string playerId,
            string playerName)
        {
            if (gameSession == null)
            {
                Debug.LogError("[SESSION] A sessão ainda não foi inicializada.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(playerId))
            {
                Debug.LogWarning("[SESSION] ID do jogador inválido.");

                return null;
            }

            if (string.IsNullOrWhiteSpace(playerName))
            {
                Debug.LogWarning("[SESSION] Nome do jogador inválido.");

                return null;
            }

            SessionPlayer existingPlayer = gameSession.GetPlayerById(playerId);

            if (existingPlayer != null)
            {
                existingPlayer.Connect();

                Debug.Log($"[SESSION] Jogador reconectado: " + $"{existingPlayer.Name} | ID: {existingPlayer.Id}");

                return existingPlayer;
            }

            SessionPlayer player = new SessionPlayer(playerId, playerName);

            gameSession.AddPlayer(player);

            Debug.Log($"[SESSION] Jogador adicionado: " + $"{player.Name} | ID: {player.Id}");

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
        
        public bool CanPlayerControlToken(
            SessionPlayer player,
            Token token)
        {
            if (gameSession == null)
                return false;

            if (player == null || token == null)
                return false;

            if (!gameSession.Players.Contains(player))
                return false;

            return player.ControlsToken(token.Id);
        }
        
        public bool HandleMoveTokenRequest(MoveTokenRequestMessage message)
        {
            if (gameSession == null || message == null)
                return false;

            SessionPlayer player = gameSession.GetPlayerById(message.PlayerId);

            Room room = gameSession.GetRoomById(message.RoomId);

            if (player == null)
            {
                Debug.LogWarning(
                    $"[MOVE] Player não encontrado: {message.PlayerId}"
                );

                return false;
            }

            if (room == null)
            {
                Debug.LogWarning(
                    $"[MOVE] Sala não encontrada: {message.RoomId}"
                );

                return false;
            }

            Token token = room.GetTokenById(message.TokenId);

            if (token == null)
            {
                Debug.LogWarning(
                    $"[MOVE] Token não encontrado: {message.TokenId}"
                );

                return false;
            }

            if (!CanPlayerControlToken(player, token))
            {
                Debug.LogWarning(
                    $"[MOVE] {player.Name} não controla {token.Name}."
                );

                return false;
            }
            
            GridCoordinate target =
                new GridCoordinate(
                    message.TargetX,
                    message.TargetY
                );

            if (!room.Grid.IsValidCoordinate(target))
            {
                Debug.LogWarning(
                    $"[MOVE] Destino fora do mapa: " +
                    $"({message.TargetX}, {message.TargetY})"
                );

                return false;
            }

            Debug.Log(
                $"[MOVE] Pedido autorizado: " +
                $"{player.Name} → {token.Name} → " +
                $"({message.TargetX}, {message.TargetY})"
            );

            return true;
        }
        
        // MÉTODOS DE TESTES DE FEATURES, SERÃO REMOVIDOS OU ATUREADOS COMPLETAMENTE DEPOIS
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
