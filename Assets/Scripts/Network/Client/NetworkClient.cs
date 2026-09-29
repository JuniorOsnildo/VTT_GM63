using System.Collections.Generic;
using Core;
using Network.DTO;
using UnityEngine;
using Network.Transport;
using Network.Messages;
using Network.Serialization;
using Session;
using Steamworks;
using VTT.Unity;

namespace Network.Client
{
    public class NetworkClient : MonoBehaviour
    {
        [SerializeField]
        private Network.Steam.SteamNetworkTransport transport;
        
        [SerializeField] private GridManager gridManager;

        private PlayerSessionState sessionState;
        
        private NetworkConnection serverConnection;
        
        private void Start()
        {
            if (transport == null)
            {
                Debug.LogError(
                    "[NETWORK CLIENT] Transport não configurado."
                );

                return;
            }
            
            if (gridManager == null)
            {
                Debug.LogError(
                    "[NETWORK CLIENT] GridManager não configurado."
                );

                return;
            }

            sessionState = new PlayerSessionState();
            
            transport.OnClientConnected += HandleConnected;
            transport.OnMessageReceived += HandleMessageReceived;
        }
        
        private void HandleConnected(NetworkConnection connection)
        {
            serverConnection = connection;
            string playerId = SteamUser.GetSteamID().ToString();
            string playerName = SteamFriends.GetPersonaName();
            
            JoinRequestMessage joinRequest =
                new JoinRequestMessage(
                    playerId,
                    playerName
                );

            string json =
                NetworkSerializer.SerializeMessage(joinRequest);

            transport.Send(
                connection,
                json
            );
        }
        
        private void HandleMessageReceived(NetworkConnection connection, string json)
        {
            NetworkMessageType messageType =
                NetworkSerializer.GetMessageType(json);

            if (messageType == NetworkMessageType.JoinAccepted)
            {
                HandleJoinAccepted(json);
            }
            else if (messageType == NetworkMessageType.SessionSnapshot)
            {
                HandleSessionSnapshot(json);
            }
            else if (messageType == NetworkMessageType.TokenCreated)
            {
                HandleTokenCreated(json);
            }
            else if (messageType == NetworkMessageType.ActiveRoomChanged)
            {
                HandleActiveRoomChanged(json);
            }
            else if (messageType == NetworkMessageType.TokenMoved)
            {
                HandleTokenMoved(json);
            }
            else if (messageType == NetworkMessageType.PlayerControlUpdated)
            {
                HandlePlayerControlUpdated(json);
            }
        }
        
        private void HandlePlayerControlUpdated(string json)
        {
            PlayerControlUpdatedMessage message = NetworkSerializer.DeserializeMessage<PlayerControlUpdatedMessage>(json);

            if (message == null)
            {
                Debug.LogWarning("[NETWORK CLIENT] PlayerControlUpdated inválido.");
                return;
            }

            sessionState.ApplyPlayerControlUpdated(message.PlayerId, message.ControlledTokenIds);
        }

        private void HandleJoinAccepted(string json)
        {
            JoinAcceptedMessage message =
                NetworkSerializer.DeserializeMessage<JoinAcceptedMessage>(
                    json
                );

            if (message == null)
            {
                Debug.LogWarning(
                    "[NETWORK CLIENT] JoinAccepted inválido."
                );

                return;
            }
            
        }
        
        private void HandleSessionSnapshot(string json)
        {
            SessionSnapshotMessage message =
                NetworkSerializer.DeserializeMessage<SessionSnapshotMessage>(
                    json
                );

            if (message == null || message.Snapshot == null)
            {
                Debug.LogWarning(
                    "[NETWORK CLIENT] SessionSnapshot inválido."
                );

                return;
            }
            
            sessionState.ApplySnapshot(
                message.Snapshot
            );

            if (message.Snapshot.ActiveRoom?.Map != null)
            {
                MapData mapData =
                    DTOConverter.ToMapData(
                        message.Snapshot.ActiveRoom.Map
                    );

                gridManager.LoadMap(mapData);
            }
            
            if (message.Snapshot.ActiveRoom?.Tokens != null)
            {
                foreach (TokenDTO tokenDTO
                         in message.Snapshot.ActiveRoom.Tokens)
                {
                    Token token =
                        DTOConverter.ToToken(tokenDTO);

                    if (token == null)
                        continue;

                    gridManager.CreateTokenVisual(token);
                }
            }
        }

        private void HandleTokenCreated(string json)
        {
            TokenCreatedMessage message =
                NetworkSerializer.DeserializeMessage<TokenCreatedMessage>(
                    json
                );

            if (message == null || message.Token == null)
            {
                Debug.LogWarning(
                    "[NETWORK CLIENT] TokenCreated inválido."
                );

                return;
            }

            if (sessionState.Snapshot == null ||
                sessionState.Snapshot.ActiveRoomId != message.RoomId)
            {
                return;
            }

            Token token =
                DTOConverter.ToToken(message.Token);

            if (token == null)
                return;

            gridManager.CreateTokenVisual(token);
        }
        
        private void HandleActiveRoomChanged(string json)
        {
            ActiveRoomChangedMessage message =
                NetworkSerializer.DeserializeMessage<ActiveRoomChangedMessage>(
                    json
                );

            if (message == null || message.Room == null)
            {
                Debug.LogWarning(
                    "[NETWORK CLIENT] ActiveRoomChanged inválido."
                );

                return;
            }

            // Atualiza a sala ativa no estado local do Player
            sessionState.ApplyActiveRoom(message.Room);

            if (message.Room.Map == null)
                return;

            MapData mapData =
                DTOConverter.ToMapData(
                    message.Room.Map
                );

            gridManager.LoadMap(mapData);

            if (message.Room.Tokens != null)
            {
                foreach (TokenDTO tokenDTO in message.Room.Tokens)
                {
                    Token token =
                        DTOConverter.ToToken(tokenDTO);

                    if (token == null)
                        continue;

                    gridManager.CreateTokenVisual(token);
                }
            }
        }
        
        private void HandleTokenMoved(string json)
        {
            TokenMovedMessage message =
                NetworkSerializer.DeserializeMessage<TokenMovedMessage>(
                    json
                );

            if (message == null)
            {
                Debug.LogWarning(
                    "[NETWORK CLIENT] TokenMoved inválido."
                );

                return;
            }

            if (sessionState.Snapshot == null ||
                sessionState.Snapshot.ActiveRoomId != message.RoomId)
            {
                return;
            }

            TokenVisual visual =
                gridManager.GetTokenVisual(message.TokenId);

            if (visual == null)
                return;

            if (message.Path == null || message.Path.Length == 0)
                return;

            List<GridCoordinate> path =
                new List<GridCoordinate>();

            foreach (PathCoordinate coordinate in message.Path)
            {
                path.Add(
                    new GridCoordinate(
                        coordinate.X,
                        coordinate.Y
                    )
                );
            }

            StartCoroutine(
                visual.MoveAlongPath(path)
            );
            
            Token token = visual.GetToken();

            if (token != null)
            {
                BoardGrid boardGrid = gridManager.GetGrid();

                if (boardGrid != null)
                {
                    GridCoordinate from = token.Coordinates;
                    GridCoordinate to = new GridCoordinate(message.X, message.Y);

                    boardGrid.MoveToken(token, from, to);
                }
                else
                {
                    token.Coordinates = new GridCoordinate(message.X, message.Y);
                }
            }
        } 
        
        public PlayerSessionState GetSessionState()
        {
            return sessionState;
        }
        
        public string GetLocalPlayerId()
        {
            return SteamUser.GetSteamID().ToString();
        }
        
        public void SendMoveTokenRequest(string roomId, string tokenId, int targetX, int targetY)
        {
            if (transport == null || serverConnection == null)
                return;

            MoveTokenRequestMessage message = new MoveTokenRequestMessage(roomId, tokenId, targetX, targetY);

            string json = NetworkSerializer.SerializeMessage(message);

            transport.Send(serverConnection, json);
        }
        
        private void OnDestroy()
        {
            if (transport == null)
                return;

            transport.OnClientConnected -= HandleConnected;
            transport.OnMessageReceived -= HandleMessageReceived;
        }
    }
}