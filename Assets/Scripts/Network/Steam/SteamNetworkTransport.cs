using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Network.Transport;
using Steamworks;

namespace Network.Steam
{
    public class SteamNetworkTransport : MonoBehaviour, INetworkTransport
    {
        private HSteamListenSocket listenSocket;
        private HSteamNetPollGroup pollGroup;

        private Callback<SteamNetConnectionStatusChangedCallback_t> connectionStatusChanged;
        
        private readonly Dictionary<string, HSteamNetConnection>
            activeConnections = new Dictionary<string, HSteamNetConnection>();
        
        private const int VirtualPort = 0;
        
        public event System.Action<NetworkConnection> OnClientConnected;
        public event System.Action<NetworkConnection> OnClientDisconnected;
        public event System.Action<NetworkConnection, string> OnMessageReceived;

        public bool IsRunning { get; private set; }

        public NetworkTransportMode Mode { get; private set; }
            = NetworkTransportMode.None;
        
        private void Update()
        {
            if (!IsRunning)
                return;

            if (Mode == NetworkTransportMode.Host)
            {
                ReceiveHostMessages();
            }
            else if (Mode == NetworkTransportMode.Client)
            {
                ReceiveClientMessages();
            }
        }
        
        private void ReceiveClientMessages()
        {
            foreach (HSteamNetConnection steamConnection
                     in activeConnections.Values)
            {
                IntPtr[] messages = new IntPtr[32];

                int messageCount =
                    SteamNetworkingSockets.ReceiveMessagesOnConnection(
                        steamConnection,
                        messages,
                        messages.Length
                    );

                if (messageCount <= 0)
                    continue;

                for (int i = 0; i < messageCount; i++)
                {
                    IntPtr messagePtr = messages[i];

                    SteamNetworkingMessage_t message =
                        Marshal.PtrToStructure<SteamNetworkingMessage_t>(
                            messagePtr
                        );

                    try
                    {
                        byte[] data =
                            new byte[message.m_cbSize];

                        Marshal.Copy(
                            message.m_pData,
                            data,
                            0,
                            message.m_cbSize
                        );

                        string text =
                            System.Text.Encoding.UTF8.GetString(data);

                        string connectionId =
                            message.m_conn.ToString();

                        NetworkConnection connection =
                            new NetworkConnection(connectionId);

                        Debug.Log(
                            $"[STEAM NETWORK] Mensagem recebida do Host: {text}"
                        );

                        OnMessageReceived?.Invoke(
                            connection,
                            text
                        );
                    }
                    finally
                    {
                        SteamNetworkingMessage_t.Release(
                            messagePtr
                        );
                    }
                }
            }
        }
        
        private void ReceiveHostMessages()
        {
            IntPtr[] messages = new IntPtr[32];

            int messageCount =
                SteamNetworkingSockets.ReceiveMessagesOnPollGroup(
                    pollGroup,
                    messages,
                    messages.Length
                );

            if (messageCount <= 0)
                return;

            for (int i = 0; i < messageCount; i++)
            {
                IntPtr messagePtr = messages[i];

                SteamNetworkingMessage_t message =
                    Marshal.PtrToStructure<SteamNetworkingMessage_t>(
                        messagePtr
                    );

                try
                {
                    byte[] data = new byte[message.m_cbSize];

                    Marshal.Copy(
                        message.m_pData,
                        data,
                        0,
                        message.m_cbSize
                    );

                    string text =
                        System.Text.Encoding.UTF8.GetString(data);

                    string connectionId =
                        message.m_conn.ToString();

                    NetworkConnection connection =
                        new NetworkConnection(connectionId);

                    Debug.Log(
                        $"[STEAM NETWORK] Mensagem recebida de " +
                        $"{connectionId}: {text}"
                    );

                    OnMessageReceived?.Invoke(
                        connection,
                        text
                    );
                }
                finally
                {
                    SteamNetworkingMessage_t.Release(
                        messagePtr
                    );
                }
            }
        }
        
        private void HandleConnected(
            SteamNetConnectionStatusChangedCallback_t callback)
        {
            string connectionId = callback.m_hConn.ToString();

            activeConnections[connectionId] =
                callback.m_hConn;

            // Apenas o HOST utiliza Poll Group.
            if (Mode == NetworkTransportMode.Host)
            {
                bool addedToPollGroup =
                    SteamNetworkingSockets.SetConnectionPollGroup(
                        callback.m_hConn,
                        pollGroup
                    );

                if (!addedToPollGroup)
                {
                    Debug.LogError(
                        $"[STEAM NETWORK] Não foi possível adicionar " +
                        $"a conexão {connectionId} ao Poll Group."
                    );

                    activeConnections.Remove(connectionId);

                    SteamNetworkingSockets.CloseConnection(
                        callback.m_hConn,
                        0,
                        "Falha ao configurar Poll Group",
                        false
                    );

                    return;
                }
            }

            NetworkConnection connection =
                new NetworkConnection(connectionId);

            if (Mode == NetworkTransportMode.Host)
            {
                Debug.Log(
                    $"[STEAM NETWORK] Cliente conectado. " +
                    $"ConnectionId: {connectionId}"
                );
            }
            else if (Mode == NetworkTransportMode.Client)
            {
                Debug.Log(
                    $"[STEAM NETWORK] Conectado ao Host. " +
                    $"ConnectionId: {connectionId}"
                );
            }

            OnClientConnected?.Invoke(connection);
        }
        
        private void HandleDisconnected(
            SteamNetConnectionStatusChangedCallback_t callback)
        {
            string connectionId = callback.m_hConn.ToString();

            activeConnections.Remove(connectionId);

            NetworkConnection connection =
                new NetworkConnection(connectionId);

            SteamNetworkingSockets.CloseConnection(
                callback.m_hConn,
                0,
                "Conexão encerrada",
                false
            );

            Debug.Log(
                $"[STEAM NETWORK] Cliente desconectado. " +
                $"ConnectionId: {connectionId}"
            );

            OnClientDisconnected?.Invoke(connection);
        }
        
        private void Awake()
        {
            connectionStatusChanged =
                Callback<SteamNetConnectionStatusChangedCallback_t>.Create(
                    OnConnectionStatusChanged
                );
        }
        
        private void OnConnectionStatusChanged(
            SteamNetConnectionStatusChangedCallback_t callback)
        {
            Debug.Log(
                $"[STEAM NETWORK] Estado da conexão: " +
                $"{callback.m_info.m_eState}"
            );

            // HOST: recebeu uma tentativa de conexão.
            if (callback.m_info.m_eState ==
                ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting &&
                Mode == NetworkTransportMode.Host)
            {
                EResult result =
                    SteamNetworkingSockets.AcceptConnection(
                        callback.m_hConn
                    );

                if (result != EResult.k_EResultOK)
                {
                    Debug.LogError(
                        $"[STEAM NETWORK] Falha ao aceitar conexão: {result}"
                    );

                    SteamNetworkingSockets.CloseConnection(
                        callback.m_hConn,
                        0,
                        "Falha ao aceitar conexão",
                        false
                    );

                    return;
                }

                Debug.Log(
                    "[STEAM NETWORK] Conexão aceita. " +
                    "Aguardando ficar Connected..."
                );
            }

            // HOST ou CLIENT: conexão estabelecida.
            if (callback.m_info.m_eState ==
                ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
            {
                HandleConnected(callback);
            }

            // HOST ou CLIENT: conexão encerrada ou perdida.
            if (callback.m_info.m_eState ==
                ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer ||
                callback.m_info.m_eState ==
                ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
            {
                HandleDisconnected(callback);
            }
        }
        
        public void StartHost()
        {
            if (IsRunning)
            {
                Debug.LogWarning(
                    "[STEAM NETWORK] O transporte já está rodando."
                );

                return;
            }

            SteamNetworkingUtils.InitRelayNetworkAccess();

            listenSocket =
                SteamNetworkingSockets.CreateListenSocketP2P(
                    VirtualPort,
                    0,
                    null
                );

            if (listenSocket == HSteamListenSocket.Invalid)
            {
                Debug.LogError(
                    "[STEAM NETWORK] Falha ao criar Listen Socket."
                );

                return;
            }

            pollGroup =
                SteamNetworkingSockets.CreatePollGroup();

            if (pollGroup == HSteamNetPollGroup.Invalid)
            {
                SteamNetworkingSockets.CloseListenSocket(
                    listenSocket
                );

                listenSocket = HSteamListenSocket.Invalid;

                Debug.LogError(
                    "[STEAM NETWORK] Falha ao criar Poll Group."
                );

                return;
            }

            Mode = NetworkTransportMode.Host;
            IsRunning = true;

            Debug.Log(
                "[STEAM NETWORK] HOST iniciado com sucesso."
            );
        }

        public void StartClient(string hostId)
        {
            if (IsRunning)
            {
                Debug.LogWarning(
                    "[STEAM NETWORK] O transporte já está rodando."
                );

                return;
            }

            if (!ulong.TryParse(hostId, out ulong steamId))
            {
                Debug.LogError(
                    $"[STEAM NETWORK] SteamID do Host inválido: {hostId}"
                );

                return;
            }

            SteamNetworkingIdentity hostIdentity =
                new SteamNetworkingIdentity();

            hostIdentity.SetSteamID64(steamId);

            HSteamNetConnection connection =
                SteamNetworkingSockets.ConnectP2P(
                    ref hostIdentity,
                    VirtualPort,
                    0,
                    null
                );

            if (connection == HSteamNetConnection.Invalid)
            {
                Debug.LogError(
                    "[STEAM NETWORK] Não foi possível iniciar " +
                    "a conexão com o Host."
                );

                return;
            }

            string connectionId = connection.ToString();

            activeConnections[connectionId] = connection;

            Mode = NetworkTransportMode.Client;
            IsRunning = true;

            Debug.Log(
                $"[STEAM NETWORK] Tentando conectar ao Host. " +
                $"ConnectionId: {connectionId}"
            );
        }

        public void Send(
            NetworkConnection connection,
            string message)
        {
            if (!IsRunning)
                return;

            if (connection == null)
                return;

            if (string.IsNullOrEmpty(message))
                return;

            if (!activeConnections.TryGetValue(
                    connection.Id,
                    out HSteamNetConnection steamConnection))
            {
                Debug.LogWarning(
                    $"[STEAM NETWORK] Conexão não encontrada: " +
                    $"{connection.Id}"
                );

                return;
            }

            byte[] data =
                System.Text.Encoding.UTF8.GetBytes(message);

            IntPtr dataPtr =
                Marshal.AllocHGlobal(data.Length);

            try
            {
                Marshal.Copy(
                    data,
                    0,
                    dataPtr,
                    data.Length
                );

                EResult result =
                    SteamNetworkingSockets.SendMessageToConnection(
                        steamConnection,
                        dataPtr,
                        (uint)data.Length,
                        Constants.k_nSteamNetworkingSend_Reliable,
                        out long messageNumber
                    );

                if (result != EResult.k_EResultOK)
                {
                    Debug.LogError(
                        $"[STEAM NETWORK] Falha ao enviar mensagem " +
                        $"para {connection.Id}: {result}"
                    );

                    return;
                }

                Debug.Log(
                    $"[STEAM NETWORK] Mensagem enviada para " +
                    $"{connection.Id}. Nº: {messageNumber}"
                );
            }
            finally
            {
                Marshal.FreeHGlobal(dataPtr);
            }
        }

        public void Broadcast(string message)
        {
            if (!IsRunning)
                return;

            if (string.IsNullOrEmpty(message))
                return;

            foreach (string connectionId in activeConnections.Keys)
            {
                NetworkConnection connection =
                    new NetworkConnection(connectionId);

                Send(
                    connection,
                    message
                );
            }
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            NetworkTransportMode previousMode = Mode;

            foreach (HSteamNetConnection connection
                     in activeConnections.Values)
            {
                SteamNetworkingSockets.CloseConnection(
                    connection,
                    0,
                    "Transporte encerrado",
                    false
                );
            }

            activeConnections.Clear();

            if (pollGroup != HSteamNetPollGroup.Invalid)
            {
                SteamNetworkingSockets.DestroyPollGroup(
                    pollGroup
                );

                pollGroup = HSteamNetPollGroup.Invalid;
            }

            if (listenSocket != HSteamListenSocket.Invalid)
            {
                SteamNetworkingSockets.CloseListenSocket(
                    listenSocket
                );

                listenSocket = HSteamListenSocket.Invalid;
            }

            IsRunning = false;
            Mode = NetworkTransportMode.None;

            Debug.Log(
                $"[STEAM NETWORK] Transporte {previousMode} encerrado."
            );
        }
    }
}