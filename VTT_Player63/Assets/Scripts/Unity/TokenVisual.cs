using System.Collections;
using System.Collections.Generic;
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
        
        public IEnumerator MoveAlongPath(
            List<GridCoordinate> path,
            float moveSpeed = 4f)
        {
            if (path == null || path.Count == 0)
                yield break;

            for (int i = 1; i < path.Count; i++)
            {
                GridCoordinate nextCoord = path[i];

                Vector3 worldPos = gridManager.GridCoordToWorldPosition(nextCoord);

                Vector3 targetPosition = new Vector3(worldPos.x, 0.3f, worldPos.z);

                while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

                    yield return null;
                }

                transform.position = targetPosition;
            }
        }
    }
}