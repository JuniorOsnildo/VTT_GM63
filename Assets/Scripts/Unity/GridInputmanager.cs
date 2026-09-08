using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;
using VTT.Unity;

namespace VTT.Unity
{
    public class GridInputManager : MonoBehaviour
    {
        private GridManager gridManager;
        private Camera mainCamera;
        private AStar pathfinder;
        private PathRenderer pathRenderer;
        
        private TokenVisual selectedToken;
        
        [SerializeField]
        private TokenCreationUI tokenCreationUI;
        
        void Start()
        {
            gridManager = GetComponent<GridManager>();
            mainCamera = Camera.main;
            pathfinder = new AStar();
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

                gridManager.CreateToken(
                    tokenCreationUI.GetPendingName(),
                    tokenCreationUI.GetPendingMovement(),
                    tokenCreationUI.GetPendingFaction(),
                    coord
                );

                tokenCreationUI.FinishPlacement();

                return;
            }

            TokenVisual clickedToken =
                hit.collider.GetComponent<TokenVisual>();

            if (clickedToken != null)
            {
                selectedToken = clickedToken;

                Debug.Log(
                    $"[TOKEN] Selecionado em {selectedToken.GetCurrentCoordinate()}"
                );

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

            GridCoordinate startCoord = selectedToken.GetCurrentCoordinate();

            BoardGrid boardGrid = gridManager.GetGrid();

            List<GridCoordinate> path = pathfinder.FindPath(startCoord, targetCoord, boardGrid);
            
            if (path == null || path.Count == 0)
            {
                pathRenderer.ClearPath();

                Debug.Log($"[CAMINHO] Não existe caminho de {startCoord} até {targetCoord}.");

                return;
            }

            int maxMovement = selectedToken.GetToken().MaxMovement;

            int totalCost = 0;

            List<GridCoordinate> validPath = new List<GridCoordinate>();

            List<GridCoordinate> invalidPath = new List<GridCoordinate>();

            // A primeira posição é a posição atual do token
            validPath.Add(path[0]);

            for (int i = 1; i < path.Count; i++)
            {
                GridCoordinate from = path[i - 1];
                GridCoordinate to = path[i];

                int movementCost = boardGrid.GetMovementCost(from, to);

                totalCost += movementCost;

                if (totalCost <= maxMovement)
                {
                    // Ainda está dentro do alcance
                    validPath.Add(to);
                }
                else
                {
                    // Passou do limite
                    invalidPath.Add(to);
                }
            }

            Debug.Log($"[CAMINHO] Custo: {totalCost}/{maxMovement}");

            pathRenderer.ClearPath();

            // Parte permitida
            if (validPath.Count > 0)
            {
                pathRenderer.DrawPath(validPath, Color.yellow);
            }

            // Parte fora do alcance
            if (invalidPath.Count > 0)
            {
                pathRenderer.DrawPath(invalidPath, Color.red);

                Debug.LogWarning($"[MOVIMENTO] O destino está fora do alcance! " + $"Custo: {totalCost} | Máximo: {maxMovement}");

                return;
            }

            // O caminho inteiro está dentro do alcance
            StartCoroutine(selectedToken.MoveAlongPath(path));
        }
    }
}