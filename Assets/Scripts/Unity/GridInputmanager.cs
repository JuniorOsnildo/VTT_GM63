using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;
using Session;

namespace VTT.Unity
{
    public class GridInputManager : MonoBehaviour
    {
        [SerializeField]
        private SessionManager sessionManager;
        
        private GridManager gridManager;
        private Camera mainCamera;
        private TokenMovementService pathfinder;
        private PathRenderer pathRenderer;
        
        private TokenVisual selectedToken;
        
        [SerializeField]
        private TokenCreationUI tokenCreationUI;
        
        void Start()
        {
            gridManager = GetComponent<GridManager>();
            mainCamera = Camera.main;
            pathfinder = new TokenMovementService();
            pathRenderer = new PathRenderer(gridManager);
        }
        
        void Update()
        {
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                HandleGridClick();
            }
        }
        
        private void HandleGridClick()
        {
            var mouse = Mouse.current;

            if (mouse == null || mainCamera == null)
                return;

            Ray ray = mainCamera.ScreenPointToRay(
                mouse.position.ReadValue()
            );

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            if (tokenCreationUI != null &&
                tokenCreationUI.IsWaitingForPlacement)
            {
                if (!hit.collider.gameObject.name.StartsWith("Cell_"))
                    return;

                Vector3 hitPoint = hit.point;

                int x = Mathf.RoundToInt(hitPoint.x);
                int y = Mathf.RoundToInt(hitPoint.z);

                GridCoordinate coord =
                    new GridCoordinate(x, y);

                Token token = sessionManager.CreateToken(
                    tokenCreationUI.GetPendingName(),
                    coord,
                    tokenCreationUI.GetPendingFaction(),
                    tokenCreationUI.GetPendingMovement()
                );

                if (token == null)
                    return;

                gridManager.CreateTokenVisual(token);

                tokenCreationUI.FinishPlacement();

                return;
            }

            TokenVisual clickedToken =
                hit.collider.GetComponent<TokenVisual>();

            if (clickedToken != null)
            {
                selectedToken = clickedToken;

                return;
            }

            if (selectedToken != null)
            {
                if (!hit.collider.gameObject.name.StartsWith("Cell_"))
                    return;

                Vector3 hitPoint = hit.point;

                int x = Mathf.RoundToInt(hitPoint.x);
                int y = Mathf.RoundToInt(hitPoint.z);

                GridCoordinate targetCoord =
                    new GridCoordinate(x, y);

                CalculateAndShowPath(targetCoord);

                selectedToken = null;
            }
        }
        private void CalculateAndShowPath(GridCoordinate targetCoord)
        {
            if (selectedToken == null)
                return;

            Token movingToken = selectedToken.GetToken();

            GridCoordinate startCoord = selectedToken.GetCurrentCoordinate();

            BoardGrid boardGrid = gridManager.GetGrid();

            TokenMovementService.TokenMovementResult result = pathfinder.EvaluateMove(movingToken, targetCoord, boardGrid);

            if (!result.HasPath)
            {
                pathRenderer.ClearPath();

                Debug.Log(
                    $"[CAMINHO] Não existe caminho de " +
                    $"{startCoord} até {targetCoord}."
                );

                return;
            }

            List<GridCoordinate> path = result.Path;

            int totalCost = result.TotalCost;

            int maxMovement = movingToken.MaxMovement;

            int currentCost = 0;

            List<GridCoordinate> validPath = new List<GridCoordinate>();

            List<GridCoordinate> invalidPath = new List<GridCoordinate>();

            // A primeira posição é a posição atual do token
            validPath.Add(path[0]);

            for (int i = 1; i < path.Count; i++)
            {
                GridCoordinate from = path[i - 1];
                GridCoordinate to = path[i];

                int movementCost = boardGrid.GetMovementCost(from, to, movingToken);

                currentCost += movementCost;

                if (currentCost <= maxMovement)
                {
                    validPath.Add(to);
                }
                else
                {
                    invalidPath.Add(to);
                }
            }

            pathRenderer.ClearPath();

            // Parte permitida
            if (validPath.Count > 0)
            {
                pathRenderer.DrawPath(
                    validPath,
                    Color.yellow
                );
            }

            // Parte fora do alcance
            if (invalidPath.Count > 0)
            {
                pathRenderer.DrawPath(
                    invalidPath,
                    Color.red
                );

                return;
            }
            
            pathfinder.ExecuteMove(movingToken, boardGrid, result);

            StartCoroutine(selectedToken.MoveAlongPath(path));
        }
    }
}