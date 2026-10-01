using System;
using Core;
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
        
        private TokenMovementService movementService;
        
        public event Action<Room> OnRoomCreated;
        public event Action<Room> OnRoomDeleted;
        public event Action<TokenMovedMessage> OnTokenMoved;
        public event Action<Token> OnTokenCreated;
        public event Action<Room> OnActiveRoomChanged;
        public event Action<SessionPlayer> OnPlayerControlUpdated;
        public event Action<TokenUpdatedMessage> OnTokenUpdated;

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
            
            movementService = new TokenMovementService();

            Room firstRoom = CreateRoom("Sala Inicial", 20, 20);
            
            SwitchRoom(firstRoom);
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
            OnRoomCreated?.Invoke(newRoom);
            
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
            OnRoomDeleted?.Invoke(room);

            if (wasActiveRoom)
            {
                SwitchRoom(gameSession.ActiveRoom);
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
            
            OnActiveRoomChanged?.Invoke(room);
        }
        
        public SessionPlayer AddPlayer(string playerId, string playerName)
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
        
        public void AssignTokenToPlayer(SessionPlayer player, string tokenId)
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
        
        public void RemoveTokenFromPlayer(SessionPlayer player, string tokenId)
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
        
        public bool CanPlayerControlToken(SessionPlayer player, Token token)
        {
            if (gameSession == null)
                return false;

            if (player == null || token == null)
                return false;

            if (!gameSession.Players.Contains(player))
                return false;

            return player.ControlsToken(token.Id);
        }
        
        public bool MoveToken(Token token, GridCoordinate target)
        {
            if (gameSession == null || token == null)
                return false;

            Room room = gameSession.ActiveRoom;

            if (room == null)
                return false;

            if (!room.Grid.IsValidCoordinate(target))
                return false;

            TokenMovementService.TokenMovementResult result =
                movementService.EvaluateMove(
                    token,
                    target,
                    room.Grid
                );

            if (!result.HasPath)
                return false;

            if (!result.CanMove)
                return false;

            movementService.ExecuteMove(
                token,
                room.Grid,
                result
            );

            TokenMovedMessage movedMessage =
                new TokenMovedMessage(
                    room.Id,
                    token.Id,
                    token.Coordinates.X,
                    token.Coordinates.Y,
                    result.Path
                );

            OnTokenMoved?.Invoke(movedMessage);

            TokenVisual visual =
                gridManager.GetTokenVisual(token.Id);

            if (visual != null)
            {
                StartCoroutine(
                    visual.MoveAlongPath(result.Path)
                );
            }

            return true;
        }
        
        public bool HandleMoveTokenRequest(SessionPlayer player,MoveTokenRequestMessage message)
        {
            if (gameSession == null || message == null)
                return false;

            Room room = gameSession.GetRoomById(message.RoomId);

            if (player == null)
            {
                Debug.LogWarning(
                    $"[MOVE] Player não encontrado"
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
            
            TokenMovementService.TokenMovementResult result = movementService.EvaluateMove(token, target, room.Grid);

            if (!result.HasPath)
            {
                Debug.LogWarning(
                    $"[MOVE] Não existe caminho para " +
                    $"{token.Name} até ({message.TargetX}, {message.TargetY})."
                );

                return false;
            }

            if (!result.CanMove)
            {
                Debug.LogWarning(
                    $"[MOVE] Destino fora do alcance de {token.Name}. " +
                    $"Custo: {result.TotalCost} | " +
                    $"Máximo: {token.MaxMovement}"
                );

                return false;
            }
            
            movementService.ExecuteMove(token, room.Grid, result);
            
            TokenMovedMessage movedMessage =
                new TokenMovedMessage(
                    room.Id,
                    token.Id,
                    token.Coordinates.X,
                    token.Coordinates.Y,
                    result.Path
                );

            OnTokenMoved?.Invoke(movedMessage);
            
            if (room == gameSession.ActiveRoom)
            {
                TokenVisual visual =
                    gridManager.GetTokenVisual(token.Id);

                if (visual != null)
                {
                    StartCoroutine(
                        visual.MoveAlongPath(result.Path)
                    );
                }
            }
            
            Debug.Log(
                $"[MOVE] Pedido autorizado: " +
                $"{player.Name} → {token.Name} → " +
                $"({message.TargetX}, {message.TargetY})"
            );

            return true;
        }
        
        public Token CreateToken(string name, GridCoordinate coordinate, Faction faction, int maxMovement)
        {
            Room activeRoom = GetActiveRoom();

            if (activeRoom == null)
            {
                Debug.LogWarning(
                    "[TOKEN] Não existe uma sala ativa."
                );

                return null;
            }

            Token token = activeRoom.CreateToken(
                name,
                coordinate,
                faction,
                maxMovement
            );

            if (token == null)
                return null;

            OnTokenCreated?.Invoke(token);

            return token;
        }
        
        public void NotifyPlayerControlUpdated(SessionPlayer player)
        {
            if (player == null)
                return;

            OnPlayerControlUpdated?.Invoke(player);
        }
        
        public bool UpdateTokenProperties(Token token, string name, int maxMovement)
        {
            if (token == null)
                return false;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (maxMovement <= 0)
                return false;

            Room activeRoom = gameSession?.ActiveRoom;

            if (activeRoom == null)
                return false;

            if (!activeRoom.Tokens.Contains(token))
                return false;

            token.UpdateProperties(name.Trim(), maxMovement);
            
            OnTokenUpdated?.Invoke(new TokenUpdatedMessage(activeRoom.Id, token.Id, token.Name, token.MaxMovement));

            return true;
        }
        
    }
}
