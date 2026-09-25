using UnityEngine;
using Network.Transport;

namespace Network.Steam
{
    public class SteamNetworkTest : MonoBehaviour
    {
        [SerializeField]
        private SteamNetworkTransport transport;

        [SerializeField]
        private bool startAsHost;

        [SerializeField]
        private string hostSteamId;

        private void Start()
        {
            if (transport == null)
            {
                Debug.LogError(
                    "[STEAM TEST] SteamNetworkTransport não configurado."
                );

                return;
            }

            if (startAsHost)
            {
                Debug.Log(
                    "[STEAM TEST] Iniciando como Host..."
                );

                transport.StartHost();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(hostSteamId))
                {
                    Debug.LogError(
                        "[STEAM TEST] SteamID do Host não informado."
                    );

                    return;
                }

                Debug.Log(
                    "[STEAM TEST] Iniciando como Client..."
                );

                transport.StartClient(hostSteamId);
            }
        }
    }
}