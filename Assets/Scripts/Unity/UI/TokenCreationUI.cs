using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.UI
{
    public class TokenCreationUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField tokenNameInput;

        [SerializeField]
        private TMP_InputField movementInput;
        
        [SerializeField]
        private TMP_Dropdown factionDropdown;
        
        [SerializeField]
        private TMP_Text statusText;

        [SerializeField]
        private Button createButton;

        private string pendingName;
        private int pendingMovement;
        private Faction pendingFaction;
        
        public bool IsWaitingForPlacement { get; private set; }

        private void Start()
        {
            createButton.onClick.AddListener(
                PrepareTokenCreation
            );
        }

        private void PrepareTokenCreation()
        {
            string tokenName =
                tokenNameInput.text.Trim();

            if (string.IsNullOrEmpty(tokenName))
            {
                Debug.LogWarning(
                    "[TOKEN] Digite um nome."
                );

                return;
            }

            if (!int.TryParse(
                    movementInput.text,
                    out int movement))
            {
                Debug.LogWarning(
                    "[TOKEN] Movimento inválido."
                );

                return;
            }

            if (movement <= 0)
            {
                Debug.LogWarning(
                    "[TOKEN] Movimento deve ser maior que zero."
                );

                return;
            }
            
            switch (factionDropdown.value)
            {
                case 0:
                    pendingFaction = Faction.Ally;
                    break;

                case 1:
                    pendingFaction = Faction.Enemie;
                    break;

                case 2:
                    pendingFaction = Faction.Neutral;
                    break;
            }

            pendingName = tokenName;
            pendingMovement = movement;
            
            IsWaitingForPlacement = true;
            
            statusText.text = $"Escolha onde posicionar o token.";
        }

        public string GetPendingName()
        {
            return pendingName;
        }

        public int GetPendingMovement()
        {
            return pendingMovement;
        }
        
        public Faction GetPendingFaction()
        {
            return pendingFaction;
        }

        public void FinishPlacement()
        {
            IsWaitingForPlacement = false;

            tokenNameInput.text = "";
            movementInput.text = "";
            
            statusText.text = "";
        }
    }
}