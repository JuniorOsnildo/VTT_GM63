using System;
using System.Collections.Generic;
using System.Linq;
using Core;

namespace Core
{
    public class AStar
    {
        private class Node
        {
            public GridCoordinate Coordinate { get; set; }
            public int GCost { get; set; } 
            public int HCost { get; set; }  
            public int FCost => GCost + HCost; 
            public Node Parent { get; set; } 
        }

        private BoardGrid _boardGrid;
        private GridCoordinate start;
        private GridCoordinate goal;
        private HashSet<GridCoordinate> closedSet;  
        private List<Node> openSet;
        
        public List<GridCoordinate> FindPath(GridCoordinate startCoord, GridCoordinate goalCoord, BoardGrid pathfindingBoardGrid)
        {
            start = startCoord;
            goal = goalCoord;
            _boardGrid = pathfindingBoardGrid;
            closedSet = new HashSet<GridCoordinate>();
            openSet = new List<Node>();
            
            var startNode = new Node
            {
                Coordinate = start,
                GCost = 0,
                HCost = start.ManhattanDistanceTo(goal),
                Parent = null
            };
            openSet.Add(startNode);
            
            while (openSet.Count > 0)
            {
                var currentNode = openSet.OrderBy(n => n.FCost).First();
                
                if (currentNode.Coordinate == goal)
                {
                    return ReconstructPath(currentNode);
                }
                
                openSet.Remove(currentNode);
                closedSet.Add(currentNode.Coordinate);
                
                var neighbors = _boardGrid.GetNeighbors(currentNode.Coordinate);
                foreach (var neighborCoord in neighbors)
                {
                    if (closedSet.Contains(neighborCoord))
                        continue;
                    
                    int movementCost = _boardGrid.GetMovementCost(currentNode.Coordinate, neighborCoord);

                    int tentativeGCost = currentNode.GCost + movementCost;

                    var existingNode = openSet.FirstOrDefault(n => n.Coordinate == neighborCoord);

                    if (existingNode != null)
                    {
                        if (tentativeGCost >= existingNode.GCost) continue;
                        existingNode.GCost = tentativeGCost;
                        existingNode.Parent = currentNode;
                    }
                    else
                    {
                        var newNode = new Node
                        {
                            Coordinate = neighborCoord,
                            GCost = tentativeGCost,
                            HCost = neighborCoord.ManhattanDistanceTo(goal),
                            Parent = currentNode
                        };
                        openSet.Add(newNode);
                    }
                }
            }

            return new List<GridCoordinate>();
        }
        
        private List<GridCoordinate> ReconstructPath(Node endNode)
        {
            var path = new List<GridCoordinate>();
            var current = endNode;

            while (current != null)
            {
                path.Add(current.Coordinate);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}