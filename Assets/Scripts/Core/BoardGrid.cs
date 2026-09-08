using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Core
{
    public class BoardGrid
    {
        public int width { get; set; }
        public int height { get; set; }
        public GridCell[,] grid;
        public Dictionary<GridCoordinate, Token> occupancy;

        private static readonly GridCoordinate[] DirectionOffsets = new[]
        {
            new GridCoordinate(1, 0), // right
            new GridCoordinate(-1, 0), // left
            new GridCoordinate(0, 1), // down
            new GridCoordinate(0, -1), // up
            new GridCoordinate(1, 1), // right-down
            new GridCoordinate(1, -1), // right-up
            new GridCoordinate(-1, 1), // left-down
            new GridCoordinate(-1, -1) // left-up
        };

        public BoardGrid(int width, int height)
        {
            this.width = width;
            this.height = height;
            
            this.grid = new GridCell[width, height];
            
            occupancy = new Dictionary<GridCoordinate, Token>();
        }

        [CanBeNull]
        public GridCell GetCell(GridCoordinate coordinate)
        {
            return grid[coordinate.X, coordinate.Y];
        }

        public bool IsValidCoordinate(GridCoordinate coordinate)
        {
            return coordinate.X >= 0 && coordinate.X < width && coordinate.Y >= 0 && coordinate.Y < height;
        }

        [CanBeNull]
        public Token GetOccupantToken(GridCoordinate coordinate)
        {
            return occupancy[coordinate];
        }

        public List<GridCoordinate> GetNeighbors(GridCoordinate coordinate)
        {
            var neighbors = new List<GridCoordinate>();

            foreach (var offset in DirectionOffsets)
            {
                var neighborCoord = coordinate + offset;
                if (!IsValidCoordinate(neighborCoord)) continue;

                var cell = GetCell(neighborCoord);
                if (cell == null || !cell.IsWalkable) continue;

                neighbors.Add(neighborCoord);
            }

            return neighbors;
        }

        [CanBeNull]
        private Token GetOccupant(GridCoordinate coord)
        {
            return occupancy.TryGetValue(coord, out var token) ? token : null;
        }

        public bool IsOccupied(GridCoordinate coord)
        {
            return GetOccupant(coord) != null;
        }

        private bool IsDiagonal(GridCoordinate from, GridCoordinate to)
        {
            int dx = Math.Abs(to.X - from.X);
            int dy = Math.Abs(to.Y - from.Y);
            return dx != 0 && dy != 0;
        }

        public int GetMovementCost(GridCoordinate from, GridCoordinate to)
        {
            var costTo = IsOccupied(to) ? (int)TerrainType.Difficult : grid[to.X, to.Y].BaseCost;

            return IsDiagonal(from, to) ? costTo * 2 : costTo;
        }

        public void SetCell(GridCoordinate coord, TerrainType type)
        {
            if (IsValidCoordinate(coord))
            {
                grid[coord.X, coord.Y] = new GridCell(coord, type);
            }
        }
    }
}