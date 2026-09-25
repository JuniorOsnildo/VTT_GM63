using System.Collections.Generic;
using Core;
using Network.DTO;
using UnityEngine;
using Network.Transport;
using Network.Messages;
using Network.Serialization;
using Session;
using VTT.Unity;

namespace Network.Client
{
    public class NetworkClient : MonoBehaviour
    {
        [SerializeField]
        private Network.Steam.SteamNetworkTransport transport;

        [SerializeField]
        private string playerId = "player_test";

        [SerializeField]
        private string playerName = "Player Test";
        
        [SerializeField] private GridManager gridManager;

        private PlayerSessionState sessionState;
        
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

        private void HandleConnected(
            NetworkConnection connection)
        {
            JoinRequestMessage joinRequest =
                new JoinRequestMessage(
                    playerId,
                    playerName
                );

            string json =
                NetworkSerializer.SerializeMessage(joinRequest);

            Debug.Log(
                $"[NETWORK CLIENT] Enviando JoinRequest: {json}"
            );

            transport.Send(
                connection,
                json
            );
        }
        
        private void HandleMessageReceived(
            NetworkConnection connection,
            string json)
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

            Debug.Log(
                $"[NETWORK CLIENT] Entrada na sessão aceita. " +
                $"PlayerId: {message.PlayerId}"
            );
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
            
            Debug.Log(
                $"[NETWORK CLIENT] SessionSnapshot recebido. " +
                $"Sala ativa: {message.Snapshot.ActiveRoomId} | " +
                $"Jogadores: {message.Snapshot.Players?.Length ?? 0} | " +
                $"Tokens: {message.Snapshot.ActiveRoom?.Tokens?.Length ?? 0}"
            );
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
                token.Coordinates =
                    new GridCoordinate(
                        message.X,
                        message.Y
                    );
            }
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