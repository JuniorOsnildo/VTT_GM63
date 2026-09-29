using System.Collections.Generic;
using Core;
using Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.UI
{
    public class TokenOptionsUI : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField]
        private SessionManager sessionManager;

        [SerializeField]
        private GameObject panel;

        [SerializeField]
        private TMP_Text tokenNameText;

        [SerializeField]
        private Transform playerListContainer;

        [SerializeField]
        private GameObject playerTogglePrefab;

        private Token currentToken;

        private readonly List<GameObject> createdPlayerEntries = new List<GameObject>();

        private readonly HashSet<SessionPlayer> modifiedPlayers = new HashSet<SessionPlayer>();
        
        private void Start()
        {
            Close();
        }

        public void Open(Token token)
        {
            if (token == null)
                return;

            currentToken = token;
            
            modifiedPlayers.Clear();

            tokenNameText.text = token.Name;

            RefreshPlayerList();

            panel.SetActive(true);
        }

        public void Close()
        {
            foreach (SessionPlayer player in modifiedPlayers)
            {
                sessionManager.NotifyPlayerControlUpdated(player);
            }

            modifiedPlayers.Clear();

            currentToken = null;

            ClearPlayerList();

            if (panel != null)
                panel.SetActive(false);
        }

        private void RefreshPlayerList()
        {
            ClearPlayerList();

            if (currentToken == null)
                return;

            GameSession session =
                sessionManager.GetGameSession();

            if (session == null)
                return;

            foreach (SessionPlayer player in session.Players)
            {
                CreatePlayerEntry(player);
            }
        }

        private void CreatePlayerEntry(SessionPlayer player)
        {
            GameObject entry =
                Instantiate(
                    playerTogglePrefab,
                    playerListContainer
                );

            createdPlayerEntries.Add(entry);

            TMP_Text playerNameText =
                entry.GetComponentInChildren<TMP_Text>();

            Toggle toggle =
                entry.GetComponentInChildren<Toggle>();

            if (playerNameText != null)
            {
                playerNameText.text =
                    player.IsConnected
                        ? player.Name
                        : $"{player.Name} (Desconectado)";
            }

            if (toggle == null)
                return;

            toggle.SetIsOnWithoutNotify(
                player.ControlsToken(currentToken.Id)
            );

            toggle.onValueChanged.AddListener(
                isOn =>
                {
                    if (isOn)
                    {
                        sessionManager.AssignTokenToPlayer(
                            player,
                            currentToken.Id
                        );
                    }
                    else
                    {
                        sessionManager.RemoveTokenFromPlayer(
                            player,
                            currentToken.Id
                        );
                    }
                    
                    modifiedPlayers.Add(player);
                }
            );
        }

        private void ClearPlayerList()
        {
            foreach (GameObject entry in createdPlayerEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }

            createdPlayerEntries.Clear();
        }
    }
}