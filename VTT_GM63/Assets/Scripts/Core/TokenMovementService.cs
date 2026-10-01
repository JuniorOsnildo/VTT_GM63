using System.Collections.Generic;

namespace Core
{
    public class TokenMovementService
    {
        private readonly AStar pathfinder;

        
        public TokenMovementService()
        {
            pathfinder = new AStar();
        }
        
        public class TokenMovementResult
        {
            public List<GridCoordinate> Path { get; private set; }
            public int TotalCost { get; private set; }
            public bool HasPath { get; private set; }
            public bool CanMove { get; private set; }

            public TokenMovementResult(
                List<GridCoordinate> path,
                int totalCost,
                bool hasPath,
                bool canMove)
            {
                Path = path;
                TotalCost = totalCost;
                HasPath = hasPath;
                CanMove = canMove;
            }
        }
        
        public void ExecuteMove(
            Token token,
            BoardGrid boardGrid,
            TokenMovementResult result)
        {
            if (token == null ||
                boardGrid == null ||
                result == null ||
                !result.CanMove ||
                result.Path == null ||
                result.Path.Count == 0)
            {
                return;
            }

            GridCoordinate destination =
                result.Path[result.Path.Count - 1];

            GridCoordinate current =
                token.Coordinates;

            boardGrid.MoveToken(
                token,
                current,
                destination
            );
        }
        
        public TokenMovementResult EvaluateMove(
            Token token,
            GridCoordinate target,
            BoardGrid boardGrid)
        {
            if (token == null || boardGrid == null)
            {
                return new TokenMovementResult(
                    null,
                    0,
                    false,
                    false
                );
            }

            List<GridCoordinate> path =
                FindPath(token, target, boardGrid);

            if (path == null || path.Count == 0)
            {
                return new TokenMovementResult(
                    path,
                    0,
                    false,
                    false
                );
            }

            int totalCost =
                CalculatePathCost(
                    path,
                    token,
                    boardGrid
                );

            bool canMove =
                totalCost <= token.MaxMovement;

            return new TokenMovementResult(
                path,
                totalCost,
                true,
                canMove
            );
        }

        public List<GridCoordinate> FindPath(
            Token token,
            GridCoordinate target,
            BoardGrid boardGrid)
        {
            if (token == null || boardGrid == null)
                return null;

            return pathfinder.FindPath(
                token.Coordinates,
                target,
                boardGrid,
                token
            );
        }

        public int CalculatePathCost(
            List<GridCoordinate> path,
            Token token,
            BoardGrid boardGrid)
        {
            if (path == null || token == null || boardGrid == null)
                return 0;

            int totalCost = 0;

            for (int i = 1; i < path.Count; i++)
            {
                GridCoordinate from = path[i - 1];
                GridCoordinate to = path[i];

                totalCost += boardGrid.GetMovementCost(
                    from,
                    to,
                    token
                );
            }

            return totalCost;
        }
    }
}