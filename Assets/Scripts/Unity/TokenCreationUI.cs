using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VTT.Unity
{
    public class TokenCreationUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField tokenNameInput;

        [SerializeField]
        private TMP_InputField movementInput;

        [SerializeField]
        private Button createButton;

        private string pendingName;
        private int pendingMovement;

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

            pendingName = tokenName;
            pendingMovement = movement;

            IsWaitingForPlacement = true;

            Debug.Log(
                $"[TOKEN] Clique em uma célula para posicionar {pendingName}."
            );
        }

        public string GetPendingName()
        {
            return pendingName;
        }

        public int GetPendingMovement()
        {
            return pendingMovement;
        }

        public void FinishPlacement()
        {
            IsWaitingForPlacement = false;

            tokenNameInput.text = "";
            movementInput.text = "";
        }
    }
}