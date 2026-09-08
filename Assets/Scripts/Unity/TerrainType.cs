using System.Collections.Generic;
using Core;
using UnityEngine;
using VTT.Unity;


namespace VTT.Unity
{
    public enum TerrainTag
    {
        Road = 5,   // Free
        Grass = 0,     // Free
        Stone = 1,  // Difficult
        Hole = 2,    // Difficult
        Tree = 3    // Blocked
    }
    
    public class TerrainDefinition
    {
        public TerrainTag Tag { get; set; }
        public TerrainType GameType { get; set; }
        public float SpawnWeight { get; set; }
        public List<TerrainTag> ForbiddenNeighbours { get; set; }
        public Color Color { get; set; }

        public TerrainDefinition(TerrainTag tag, TerrainType gameType, float spawnWeight, Color color,
            List<TerrainTag> forbiddenNeighbours = null)
        {
            Tag = tag;
            GameType = gameType;
            SpawnWeight = spawnWeight;
            Color = color;
            ForbiddenNeighbours = forbiddenNeighbours ?? new List<TerrainTag>();
        }
    }
    
}