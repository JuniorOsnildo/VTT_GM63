using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VTT.Unity
{
    public class GMIdentityUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text steamIdText;
        [SerializeField] private Button copyButton;

        private string steamId;

        private void Start()
        {
            steamId = Steamworks.SteamUser.GetSteamID().m_SteamID.ToString();

            steamIdText.text = $"ID: {steamId}";

            if (copyButton != null)
                copyButton.onClick.AddListener(CopySteamId);
        }

        private void OnDestroy()
        {
            if (copyButton != null)
                copyButton.onClick.RemoveListener(CopySteamId);
        }

        private void CopySteamId()
        {
            GUIUtility.systemCopyBuffer = steamId;
        }
    }
}