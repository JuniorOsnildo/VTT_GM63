using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;
using Network.Client;
using VTT.Unity;

namespace VTT.Player.Unity
{
    public class PlayerGridInputManager : MonoBehaviour
    {
        [SerializeField] private NetworkClient networkClient;

        private GridManager gridManager;
        private Camera mainCamera;
        private TokenMovementService pathfinder;
        private PathRenderer pathRenderer;

        private TokenVisual selectedToken;
        private GridCoordinate? pendingMoveTarget;

        private void Start()
        {
            gridManager = GetComponent<GridManager>();
            mainCamera = Camera.main;

            pathfinder = new TokenMovementService();
            pathRenderer = new PathRenderer(gridManager);
        }

        private void Update()
        {
            var mouse = Mouse.current;

            if (mouse == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame)
                HandleGridClick();
        }

        private void HandleGridClick()
        {
            var mouse = Mouse.current;

            if (mouse == null || mainCamera == null)
                return;

            Ray ray = mainCamera.ScreenPointToRay(mouse.position.ReadValue());

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return;

            TokenVisual clickedToken = hit.collider.GetComponent<TokenVisual>();

            if (clickedToken != null)
            {
                SelectToken(clickedToken);
                return;
            }

            if (selectedToken == null)
                return;

            if (!hit.collider.gameObject.name.StartsWith("Cell_"))
                return;

            Vector3 hitPoint = hit.point;

            int x = Mathf.RoundToInt(hitPoint.x);
            int y = Mathf.RoundToInt(hitPoint.z);

            GridCoordinate targetCoord = new GridCoordinate(x, y);

            if (pendingMoveTarget.HasValue && pendingMoveTarget.Value.Equals(targetCoord))
            {
                ConfirmPendingMove();
                return;
            }

            pendingMoveTarget = targetCoord;

            CalculateAndShowPath(targetCoord);
        }

        private void SelectToken(TokenVisual tokenVisual)
        {
            if (tokenVisual == null || networkClient == null)
                return;

            Token token = tokenVisual.GetToken();

            if (token == null)
                return;

            PlayerSessionState sessionState = networkClient.GetSessionState();

            if (sessionState == null)
                return;

            string playerId = networkClient.GetLocalPlayerId();

            if (!sessionState.CanPlayerControlToken(playerId, token.Id))
                return;

            selectedToken = tokenVisual;
            pendingMoveTarget = null;
            pathRenderer.ClearPath();
        }

        private void ConfirmPendingMove()
        {
            if (selectedToken == null || !pendingMoveTarget.HasValue)
                return;

            if (networkClient == null)
                return;

            Token movingToken = selectedToken.GetToken();

            if (movingToken == null)
                return;

            PlayerSessionState sessionState = networkClient.GetSessionState();

            if (sessionState == null || sessionState.Snapshot == null)
                return;

            string roomId = sessionState.Snapshot.ActiveRoomId;

            if (string.IsNullOrEmpty(roomId))
                return;

            GridCoordinate target = pendingMoveTarget.Value;

            networkClient.SendMoveTokenRequest(roomId, movingToken.Id, target.X, target.Y);

            pathRenderer.ClearPath();

            pendingMoveTarget = null;
            selectedToken = null;
        }

        private void CalculateAndShowPath(GridCoordinate targetCoord)
        {
            if (selectedToken == null)
                return;

            Token movingToken = selectedToken.GetToken();

            if (movingToken == null)
                return;

            BoardGrid boardGrid = gridManager.GetGrid();

            if (boardGrid == null)
                return;

            TokenMovementService.TokenMovementResult result = pathfinder.EvaluateMove(movingToken, targetCoord, boardGrid);

            if (!result.HasPath)
            {
                pathRenderer.ClearPath();
                return;
            }

            List<GridCoordinate> path = result.Path;

            int maxMovement = movingToken.MaxMovement;
            int currentCost = 0;

            List<GridCoordinate> validPath = new List<GridCoordinate>();
            List<GridCoordinate> invalidPath = new List<GridCoordinate>();

            validPath.Add(path[0]);

            for (int i = 1; i < path.Count; i++)
            {
                GridCoordinate from = path[i - 1];
                GridCoordinate to = path[i];

                int movementCost = boardGrid.GetMovementCost(from, to, movingToken);

                currentCost += movementCost;

                if (currentCost <= maxMovement)
                    validPath.Add(to);
                else
                    invalidPath.Add(to);
            }

            pathRenderer.ClearPath();

            if (validPath.Count > 0)
                pathRenderer.DrawPath(validPath, Color.yellow);

            if (invalidPath.Count > 0)
                pathRenderer.DrawPath(invalidPath, Color.red);
        }
    }
}