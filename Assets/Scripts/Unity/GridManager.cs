using System.Collections.Generic;
using UnityEngine;
using Core;
using Session;
using VTT.Unity;

namespace VTT.Unity
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private SessionManager sessionManager;
        
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float cellSize = 1f;
        
        private TokenVisual playerToken;
        private BoardGrid boardGrid;
        private MapGenerator mapGenerator;
        private GameObject gridVisuals;
        
        void Start()
        {
            // Gera o mapa procedural
            mapGenerator = new MapGenerator(gridWidth, gridHeight);
            
            boardGrid = mapGenerator.GenerateMap();
            
            sessionManager.InitializeSession("Sala Inicial", boardGrid, mapGenerator);
            
            // Renderiza
            RenderGrid();
        }
        
        public TokenVisual CreateToken(string tokenName, int maxMovement,Faction faction, GridCoordinate coord)
        {
            GameObject tokenObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            tokenObj.name = $"Token_{tokenName}";

            Renderer renderer = tokenObj.GetComponent<Renderer>();

            renderer.material.color = SetFactionColor(faction);

            tokenObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Token token = new Token(tokenName, coord, faction, maxMovement);

            TokenVisual visual = tokenObj.AddComponent<TokenVisual>();

            visual.Initialize(token, this);

            Physics.SyncTransforms();

            boardGrid.PlaceToken(token,coord);
            
            return visual;
        }

        private Color SetFactionColor(Faction faction)
        {
            switch (faction)
            {
                case Faction.Ally: return Color.cornflowerBlue;
                case Faction.Neutral: return Color.white;
                case Faction.Enemie: return Color.red;
                default: return Color.blueViolet;
            }
        }

        public TokenVisual GetVisualToken()
        {
            return playerToken;
        }
        
        private void RenderGrid()
        {
            gridVisuals = new GameObject("GridVisuals");
            gridVisuals.transform.parent = transform;
    
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var coord = new GridCoordinate(x, y);
                    
                    GameObject cellVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cellVisual.name = $"Cell_{x}_{y}";
                    cellVisual.transform.parent = gridVisuals.transform;
                    
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

        public void LoadRoom(Room room)
        {
            if (room == null)
            {
                Debug.LogWarning(
                    "[GRID] Tentativa de carregar uma sala nula."
                );

                return;
            }
            
            if (gridVisuals != null)
            {
                Destroy(gridVisuals);
                gridVisuals = null;
            }
            
            boardGrid = room.Grid;
            mapGenerator = room.MapGenerator;
            
            gridWidth = boardGrid.width;
            gridHeight = boardGrid.height;
            
            RenderGrid();

            Debug.Log(
                $"[GRID] Sala carregada: {room.Name}"
            );
        }
        
        public BoardGrid GetGrid() => boardGrid;
        public Vector3 GridCoordToWorldPosition(GridCoordinate coord) 
            => new Vector3(coord.X * cellSize, 0, coord.Y * cellSize);
    }
}