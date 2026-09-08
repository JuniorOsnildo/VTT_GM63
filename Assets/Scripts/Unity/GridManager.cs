using System.Collections.Generic;
using UnityEngine;
using Core;
using VTT.Unity;

namespace VTT.Unity
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float cellSize = 1f;
        
        private TokenVisual playerToken;
        private BoardGrid boardGrid;
        private MapGenerator mapGenerator;
        
        void Start()
        {
            // Gera o mapa procedural
            mapGenerator = new MapGenerator(gridWidth, gridHeight);
            boardGrid = mapGenerator.GenerateMap();
            
            // Renderiza
            RenderGrid();
        }
        
        public TokenVisual CreateToken(string tokenName, int maxMovement,Faction faction, GridCoordinate coord)
        {
            GameObject tokenObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            tokenObj.name = $"Token_{tokenName}";

            Renderer renderer = tokenObj.GetComponent<Renderer>();

            renderer.material.color = Color.cornflowerBlue;

            tokenObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Token token = new Token(tokenName, coord, faction, maxMovement);

            TokenVisual visual = tokenObj.AddComponent<TokenVisual>();

            visual.Initialize(token, this);

            Physics.SyncTransforms();

            return visual;
        }

        public TokenVisual GetPlayerToken()
        {
            return playerToken;
        }
        
        private void RenderGrid()
        {
            GameObject gridVisualsParent = new GameObject("GridVisuals");
            gridVisualsParent.transform.parent = transform;
    
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var coord = new GridCoordinate(x, y);
                    
                    GameObject cellVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cellVisual.name = $"Cell_{x}_{y}";
                    cellVisual.transform.parent = gridVisualsParent.transform;
                    
                    cellVisual.transform.position = new Vector3(x * cellSize, 0, y * cellSize);
                    cellVisual.transform.localScale = new Vector3(cellSize * 0.9f, 0.3f, cellSize * 0.9f);
                    
                    Renderer renderer = cellVisual.GetComponent<Renderer>();
                    TerrainTag terrainTag = mapGenerator.GetTerrainAt(coord);
                    Color terrainColor = mapGenerator.GetTerrainColor(terrainTag);
                    renderer.material.color = terrainColor;
                }
            }
            
            Physics.SyncTransforms();
        }
        
        public BoardGrid GetGrid() => boardGrid;
        public Vector3 GridCoordToWorldPosition(GridCoordinate coord) 
            => new Vector3(coord.X * cellSize, 0, coord.Y * cellSize);
    }
}