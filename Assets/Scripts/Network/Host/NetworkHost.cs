using System.Collections.Generic;
using Core;
using Network.DTO;
using UnityEngine;
using Network.Transport;
using Network.Messages;
using Network.Serialization;
using Network.Steam;
using Session;

namespace Network.Host
{
    public class NetworkHost : MonoBehaviour
    {
        [SerializeField]
        private SteamNetworkTransport transport;

        [SerializeField]
        private SessionManager sessionManager;
        
        private readonly Dictionary<string, SessionPlayer>
            playersByConnection = new Dictionary<string, SessionPlayer>();

        private void Start()
        {
            if (transport == null)
            {
                Debug.LogError(
                    "[NETWORK HOST] Transport não configurado."
                );

                return;
            }

            if (sessionManager == null)
            {
                Debug.LogError(
                    "[NETWORK HOST] SessionManager não configurado."
                );

                return;
            }

            transport.OnMessageReceived += HandleMessageReceived;
            transport.OnClientDisconnected += HandleClientDisconnected;
            sessionManager.OnTokenCreated += HandleTokenCreated;
            sessionManager.OnActiveRoomChanged += HandleActiveRoomChanged;
            sessionManager.OnTokenMoved += HandleTokenMoved;
            sessionManager.OnPlayerControlUpdated += HandlePlayerControlUpdated;
        }
        
        private void HandlePlayerControlUpdated(SessionPlayer player)
        {
            if (player == null)
                return;

            PlayerControlUpdatedMessage message = new PlayerControlUpdatedMessage(player.Id, player.ControlledTokenIds.ToArray());

            string json = NetworkSerializer.SerializeMessage(message);

            transport.Broadcast(json);
        }
        
        private void HandleTokenMoved(TokenMovedMessage message)
        {
            if (message == null)
                return;

            string json =
                NetworkSerializer.SerializeMessage(message);

            transport.Broadcast(json);
        }
        
        private void HandleActiveRoomChanged(Room room)
        {
            if (room == null)
                return;

            RoomDTO roomDTO =
                DTOConverter.ToDTO(room);

            ActiveRoomChangedMessage message =
                new ActiveRoomChangedMessage(roomDTO);

            string json =
                NetworkSerializer.SerializeMessage(message);

            transport.Broadcast(json);
        }
        
        private void HandleTokenCreated(Token token)
        {
            Room activeRoom = sessionManager.GetActiveRoom();

            if (activeRoom == null || token == null)
                return;

            TokenDTO tokenDTO = DTOConverter.ToDTO(token);

            TokenCreatedMessage message =
                new TokenCreatedMessage(
                    activeRoom.Id,
                    tokenDTO
                );

            string json =
                NetworkSerializer.SerializeMessage(message);

            transport.Broadcast(json);
        }

        private void HandleMessageReceived(NetworkConnection connection, string json)
        {
            NetworkMessageType messageType =
                NetworkSerializer.GetMessageType(json);

            if (messageType == NetworkMessageType.JoinRequest)
            {
                HandleJoinRequest(connection, json);
            }
            else if (messageType == NetworkMessageType.MoveTokenRequest)
            {
                HandleMoveTokenRequest(connection, json);
            }
        }
        
        private void HandleMoveTokenRequest(NetworkConnection connection, string json)
        {
            MoveTokenRequestMessage message =
                NetworkSerializer.DeserializeMessage<MoveTokenRequestMessage>(
                    json
                );

            if (message == null)
            {
                Debug.LogWarning(
                    "[NETWORK HOST] MoveTokenRequest inválido."
                );

                return;
            }

            if (!playersByConnection.TryGetValue(
                    connection.Id,
                    out SessionPlayer player))
            {
                Debug.LogWarning(
                    "[NETWORK HOST] MoveTokenRequest recebido de uma conexão sem jogador."
                );

                return;
            }

            Debug.Log(
                $"[NETWORK HOST] MoveTokenRequest recebido: " +
                $"{player.Name} → {message.TokenId} → " +
                $"({message.TargetX}, {message.TargetY})"
            );
            
            sessionManager.HandleMoveTokenRequest(
                player,
                message
            );
        }

        private void HandleJoinRequest(NetworkConnection connection, string json)
        {
            JoinRequestMessage message =
                NetworkSerializer.DeserializeMessage<JoinRequestMessage>(
                    json
                );

            if (message == null)
            {
                Debug.LogWarning(
                    "[NETWORK HOST] JoinRequest inválido."
                );

                return;
            }

            Debug.Log(
                $"[NETWORK HOST] JoinRequest recebido | " +
                $"PlayerId: {message.PlayerId} | " +
                $"Nome: {message.PlayerName}"
            );

            SessionPlayer player =
                sessionManager.AddPlayer(
                    message.PlayerId,
                    message.PlayerName
                );

            if (player == null)
            {
                Debug.LogWarning(
                    "[NETWORK HOST] Não foi possível adicionar o jogador."
                );

                return;
            }

            playersByConnection[connection.Id] = player;

            Debug.Log(
                $"[NETWORK HOST] Conexão {connection.Id} " +
                $"associada ao jogador {player.Name}."
            );

            // Primeiro confirma que o jogador entrou na sessão.
            JoinAcceptedMessage acceptedMessage =
                new JoinAcceptedMessage(player.Id);

            string responseJson =
                NetworkSerializer.SerializeMessage(
                    acceptedMessage
                );

            transport.Send(
                connection,
                responseJson
            );

            Debug.Log(
                $"[NETWORK HOST] JoinAccepted enviado para: " +
                $"{player.Name}"
            );

            // Depois envia o estado atual da sessão.
            SessionSnapshotDTO snapshot =
                DTOConverter.ToDTO(
                    sessionManager.GetGameSession()
                );

            SessionSnapshotMessage snapshotMessage =
                new SessionSnapshotMessage(snapshot);

            string snapshotJson =
                NetworkSerializer.SerializeMessage(
                    snapshotMessage
                );

            transport.Send(
                connection,
                snapshotJson
            );

            Debug.Log(
                $"[NETWORK HOST] SessionSnapshot enviado para: " +
                $"{player.Name}"
            );
        }
        
        private void HandleClientDisconnected(NetworkConnection connection)
        {
            if (!playersByConnection.TryGetValue(
                    connection.Id,
                    out SessionPlayer player))
            {
                return;
            }

            player.Disconnect();

            playersByConnection.Remove(
                connection.Id
            );

            Debug.Log(
                $"[NETWORK HOST] Jogador desconectado: " +
                $"{player.Name}"
            );
        }

        private void OnDestroy()
        {
            if (transport == null)
                return;

            transport.OnMessageReceived -= HandleMessageReceived;
            transport.OnClientDisconnected -= HandleClientDisconnected;

            if (sessionManager != null)
            {
                sessionManager.OnTokenCreated -= HandleTokenCreated;
                sessionManager.OnActiveRoomChanged -= HandleActiveRoomChanged;
                sessionManager.OnTokenMoved -= HandleTokenMoved;
                sessionManager.OnPlayerControlUpdated -= HandlePlayerControlUpdated;
            }
        }
    }
}