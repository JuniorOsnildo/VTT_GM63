using UnityEngine;
using Steamworks;

namespace Network.Steam
{
    public class SteamInitializer : MonoBehaviour
    {
        private bool initialized;

        private void Awake()
        {
            try
            {
                if (!SteamAPI.Init())
                {
                    Debug.LogError(
                        "[STEAM] SteamAPI.Init() falhou."
                    );

                    return;
                }

                initialized = true;

                Debug.Log(
                    "[STEAM] Steam inicializada com sucesso."
                );
            }
            catch (System.Exception exception)
            {
                Debug.LogError(
                    $"[STEAM] Erro ao inicializar: {exception}"
                );
            }
        }

        private void Update()
        {
            if (initialized)
            {
                SteamAPI.RunCallbacks();
            }
        }

        private void OnApplicationQuit()
        {
            ShutdownSteam();
        }

        private void OnDestroy()
        {
            ShutdownSteam();
        }

        private void ShutdownSteam()
        {
            if (!initialized)
                return;

            Debug.Log("[STEAM] Encerrando Steam API...");

            SteamAPI.Shutdown();
            initialized = false;
        }
        
        
    }
}