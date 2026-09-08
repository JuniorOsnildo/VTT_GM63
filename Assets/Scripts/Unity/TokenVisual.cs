using UnityEngine;
using Core;
using VTT.Unity;


namespace VTT.Unity
{
    public class TokenVisual : MonoBehaviour
    {
        private Token token;
        private GridManager gridManager;
        
        public void Initialize(Token tokenData, GridManager gm)
        {
            token = tokenData;
            gridManager = gm;

            UpdatePosition();
        }
        
        private void UpdatePosition()
        {
            Vector3 worldPos =
                gridManager.GridCoordToWorldPosition(
                    token.Coordinates
                );

            transform.position =
                new Vector3(
                    worldPos.x,
                    0.3f,
                    worldPos.z
                );
        }
        public Token GetToken()
        {
            return token;
        }

        public GridCoordinate GetCurrentCoordinate()
        {
            return token.Coordinates;
        }
    }
}