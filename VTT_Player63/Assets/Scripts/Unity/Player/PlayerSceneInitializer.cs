using UnityEngine;
using Network.Client;
using VTT.Player.Unity;

namespace VTT.Unity
{
    public class PlayerSceneInitializer : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        
        [SerializeField] private PlayerGridInputManager playerGridInputManager;

        private void Start()
        {
            NetworkClient networkClient = FindFirstObjectByType<NetworkClient>();

            if (networkClient == null)
            {
                Debug.LogError("[PLAYER] NetworkClient não encontrado.");
                return;
            }

            networkClient.SetGridManager(gridManager);
            playerGridInputManager.SetNetworkClient(networkClient);
        }
    }
}