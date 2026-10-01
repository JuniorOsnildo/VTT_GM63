using System;

namespace Network.Transport
{
    public interface INetworkTransport
    {
        event Action<NetworkConnection> OnClientConnected;

        event Action<NetworkConnection> OnClientDisconnected;

        event Action<NetworkConnection, string> OnMessageReceived;
        
        bool IsRunning { get; }
        
        NetworkTransportMode Mode { get; }

        void StartHost();

        void StartClient(string hostId);

        void Send(
            NetworkConnection connection,
            string message
        );

        void Broadcast(string message);

        void Stop();
    }
}