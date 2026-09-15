using Core;
using VTT.Unity;

namespace Session
{
    public class MapData
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TerrainTag[,] Terrain { get; private set; }

        public MapData(
            int width,
            int height,
            TerrainTag[,] terrain)
        {
            Width = width;
            Height = height;
            Terrain = terrain;
        }

        public TerrainTag GetTerrainAt(
            GridCoordinate coord)
        {
            return Terrain[
                coord.X,
                coord.Y
            ];
        }
    }
}