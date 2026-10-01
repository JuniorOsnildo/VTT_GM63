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
    
    public static class TerrainDefinitions
    {
        private static readonly Dictionary<TerrainTag, TerrainDefinition> definitions =
            new()
            {
                {
                    TerrainTag.Grass,
                    new TerrainDefinition(
                        TerrainTag.Grass,
                        TerrainType.Free,
                        0.50f,
                        new Color(0.2f, 0.8f, 0.2f)
                    )
                },

                {
                    TerrainTag.Road,
                    new TerrainDefinition(
                        TerrainTag.Road,
                        TerrainType.Free,
                        0f,
                        new Color(0.6f, 0.5f, 0.3f)
                    )
                },

                {
                    TerrainTag.Stone,
                    new TerrainDefinition(
                        TerrainTag.Stone,
                        TerrainType.Difficult,
                        0.25f,
                        new Color(0.5f, 0.5f, 0.5f)
                    )
                },

                {
                    TerrainTag.Hole,
                    new TerrainDefinition(
                        TerrainTag.Hole,
                        TerrainType.Difficult,
                        0f,
                        new Color(0.3f, 0.2f, 0.1f)
                    )
                },

                {
                    TerrainTag.Tree,
                    new TerrainDefinition(
                        TerrainTag.Tree,
                        TerrainType.Blocked,
                        0.25f,
                        new Color(0f, 0.3f, 0f)
                    )
                }
            };

        public static TerrainDefinition Get(TerrainTag tag)
        {
            return definitions[tag];
        }
    }
    
}