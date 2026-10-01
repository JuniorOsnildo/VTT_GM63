using UnityEngine;
using Network.Steam;

namespace Network.Host
{
    public class GMNetworkInitializer : MonoBehaviour
    {
        [SerializeField] private SteamNetworkTransport transport;

        private void Start()
        {
            if (transport == null)
            {
                Debug.LogError("[GM NETWORK] SteamNetworkTransport não configurado.");
                return;
            }

            Debug.Log($"[GM NETWORK] SteamID do Host: {Steamworks.SteamUser.GetSteamID().m_SteamID}");

            transport.StartHost();
        }
    }
}