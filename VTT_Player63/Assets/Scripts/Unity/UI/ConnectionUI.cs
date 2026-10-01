using TMPro;
using UnityEngine;
using Network.Steam;
using UnityEngine.SceneManagement;
using Network.Transport;

namespace VTT.Unity
{
    public class ConnectionUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField steamIdInput;
        [SerializeField] private SteamNetworkTransport transport;
        
        private void OnEnable()
        {
            if (transport != null)
                transport.OnClientConnected += HandleConnected;
        }

        private void OnDisable()
        {
            if (transport != null)
                transport.OnClientConnected -= HandleConnected;
        }

        private void HandleConnected(NetworkConnection connection)
        {
            SceneManager.LoadScene("PlayerScene");
        }

        public void Connect()
        {
            Debug.Log("[CONNECTION UI] Botão Entrar clicado.");
            
            string steamId = steamIdInput.text.Trim();

            if (string.IsNullOrEmpty(steamId))
                return;

            transport.StartClient(steamId);
        }
    }
}