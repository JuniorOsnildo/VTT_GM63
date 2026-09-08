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
        private bool tokenSelected = false;
        
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
            if (mouse == null) return;

            Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"[RAYCAST] Bateu em: {hit.collider.gameObject.name}");
        
                if (hit.collider.gameObject.name == "PlayerToken")
                {
                    tokenSelected = true;
                    Debug.Log("[TOKEN] Selecionado! Clique em uma célula de destino.");
                    return;
                }

                if (tokenSelected)
                {
                    Vector3 hitPoint = hit.point;
                    int x = Mathf.RoundToInt(hitPoint.x);
                    int y = Mathf.RoundToInt(hitPoint.z);
            
                    var targetCoord = new GridCoordinate(x, y);
                    Debug.Log($"[DESTINO] Célula selecionada: {targetCoord}");
                    CalculateAndShowPath(targetCoord);
                    tokenSelected = false;
                }
                else
                {
                    Debug.Log("[AVISO] Selecione o token primeiro!");
                }
            }
            else
            {
                Debug.Log("[ERRO] Raycast não bateu em nada!");
            }
        }
        
        
        private void CalculateAndShowPath(GridCoordinate targetCoord)
        {
            TokenVisual token = gridManager.GetPlayerToken();

            if (token == null)
            {
                Debug.LogError("[ERRO] Token não encontrado!");
                return;
            }

            GridCoordinate startCoord = token.GetCurrentCoordinate();

            Debug.Log($"[A*] Calculando caminho de {startCoord} até {targetCoord}");

            BoardGrid boardGrid = gridManager.GetGrid();

            List<GridCoordinate> path =
                pathfinder.FindPath(startCoord, targetCoord, boardGrid);

            if (path == null || path.Count == 0)
            {
                Debug.Log($"[ERRO] Sem caminho de {startCoord} até {targetCoord}");
                pathRenderer.ClearPath();
                return;
            }

            int totalCost = 0;

            for (int i = 1; i < path.Count; i++)
            {
                GridCoordinate from = path[i - 1];
                GridCoordinate to = path[i];

                totalCost += boardGrid.GetMovementCost(from, to);
            }

            int steps = path.Count - 1;

            Debug.Log(
                $"[CAMINHO] Passos: {steps} | Custo total: {totalCost}"
            );

            pathRenderer.DrawPath(path);
        }
    }
}