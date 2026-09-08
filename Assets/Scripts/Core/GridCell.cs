namespace Core
{
    public class GridCell
    {
        public readonly GridCoordinate Coordinate;
        public TerrainType TerrainType;
        public int BaseCost;
        public readonly bool IsWalkable;

        public GridCell(GridCoordinate coordinate, TerrainType terrainType)
        {
            Coordinate = coordinate;
            TerrainType = terrainType;
            BaseCost = (int)terrainType;
            IsWalkable = terrainType != TerrainType.Blocked;
        }

    }
}
