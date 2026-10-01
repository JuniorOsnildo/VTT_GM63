using System.Collections.Generic;
using Core;
using Session;
using UnityEngine;

namespace VTT.Unity
{
    public enum RoadPattern
    {
        Straight = 0,
        Cross = 1, 
        Tshape = 2 
    }
    
    public class MapGenerator
    {
        private int width;
        private int height;
        private TerrainTag[,] terrainMap;
        private RoadPattern roadPattern;

        public MapGenerator(int w, int h)
        {
            width = w;
            height = h;
            roadPattern = (RoadPattern)Random.Range(0,3);
            terrainMap = new TerrainTag[w, h];
        }
        
        public TerrainTag GetTerrainAt(GridCoordinate coord)
        {
            if (coord.X >= 0 && coord.X < width && coord.Y >= 0 && coord.Y < height)
                return terrainMap[coord.X, coord.Y];
            return TerrainTag.Grass;
        }
        
        public BoardGrid GenerateMap()
        {
            DrawRoadPattern();
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (terrainMap[x, y] == TerrainTag.Road)
                        continue;
                    
                    terrainMap[x, y] = GetRandomTerrainWithConstraints(x, y);
                }
            }
            
            return ConvertToGrid();
        }

        private void DrawRoadPattern()
        {
            int midX = width / 2;
            int midY = height / 2;
    
            switch (roadPattern)
            {
                case RoadPattern.Straight:
                    
                    for (int x = 0; x < width; x++)
                    {
                        terrainMap[x, midY - 1] = TerrainTag.Road;
                        terrainMap[x, midY] = TerrainTag.Road;
                    }
                    break;
            
                case RoadPattern.Cross:
                    
                    for (int x = 0; x < width; x++)
                    {
                        terrainMap[x, midY - 1] = TerrainTag.Road;
                        terrainMap[x, midY] = TerrainTag.Road;
                    }
                    
                    for (int y = 0; y < height; y++)
                    {
                        terrainMap[midX - 1, y] = TerrainTag.Road;
                        terrainMap[midX, y] = TerrainTag.Road;
                    }
                    break;
            
                case RoadPattern.Tshape:
                    
                    for (int x = 0; x < width; x++)
                    {
                        terrainMap[x, midY - 1] = TerrainTag.Road;
                        terrainMap[x, midY] = TerrainTag.Road;
                    }
                    
                    for (int y = 0; y < midY + 1; y++)
                    {
                        terrainMap[midX - 1, y] = TerrainTag.Road;
                        terrainMap[midX, y] = TerrainTag.Road;
                    }
                    break;
            }
        }
        
        private BoardGrid ConvertToGrid()
        {
            var boardGrid = new BoardGrid(width, height);
    
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var coord = new GridCoordinate(x, y);
                    TerrainTag tag = terrainMap[x, y];
                    TerrainType type = TerrainDefinitions.Get(tag).GameType;
                    
                    boardGrid.SetCell(coord, type);
                }
            }
    
            return boardGrid;
        }
        
        private TerrainTag GetRandomTerrainWithConstraints(int x, int y)
        {
            
            float rand = Random.Range(0.0f, 1f);

            float[] Weight =
            {
                TerrainDefinitions.Get(TerrainTag.Grass).SpawnWeight,
                TerrainDefinitions.Get(TerrainTag.Stone).SpawnWeight,
                TerrainDefinitions.Get(TerrainTag.Hole).SpawnWeight,
                TerrainDefinitions.Get(TerrainTag.Tree).SpawnWeight
            };

            float pool = 0.0f;

            for (int i = 0; i < Weight.Length; i++)
            {
                pool += Weight[i];

                if (rand < pool)
                {
                    return (TerrainTag)i;
                }
            }
            return TerrainTag.Grass;
        }

        public MapData GetMapData()
        {
            return new MapData(width, height, terrainMap);
        }
    }
}