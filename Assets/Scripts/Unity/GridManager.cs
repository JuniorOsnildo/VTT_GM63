using System.Collections.Generic;
using UnityEngine;
using Core;
using Session;
using UnityEngine.LightTransport.PostProcessing;
using VTT.Unity;

namespace VTT.Unity
{
    public class GridManager : MonoBehaviour
    {
        //[SerializeField] private SessionManager sessionManager;
        
        [SerializeField] private int gridWidth = 10;
        [SerializeField] private int gridHeight = 10;
        [SerializeField] private float cellSize = 1f;
        
        private TokenVisual playerToken;
        private BoardGrid boardGrid;
        private GameObject gridVisuals;
        private MapData currentMapData;
        
        public TokenVisual CreateTokenVisual(Token token)
        {
            GameObject tokenObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            tokenObj.name = $"Token_{token.Name}";

            Renderer renderer = tokenObj.GetComponent<Renderer>();

            renderer.material.color = SetFactionColor(token.Faction);

            tokenObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            TokenVisual visual = tokenObj.AddComponent<TokenVisual>();

            visual.Initialize(token, this);

            Physics.SyncTransforms();

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
                    TerrainTag terrainTag = currentMapData.GetTerrainAt(coord);
                    Color terrainColor = GetTerrainColor(terrainTag);
                    renderer.material.color = terrainColor;
                }
            }
            Physics.SyncTransforms();
        }
        
        private Color GetTerrainColor(TerrainTag tag)
        {
            return tag switch
            {
                TerrainTag.Grass => new Color(0.2f, 0.8f, 0.2f),
                TerrainTag.Road => new Color(0.6f, 0.5f, 0.3f),
                TerrainTag.Stone => new Color(0.5f, 0.5f, 0.5f),
                TerrainTag.Hole => new Color(0.3f, 0.2f, 0.1f),
                TerrainTag.Tree => new Color(0f, 0.3f, 0f),
                _ => Color.white
            };
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
            currentMapData = room.MapData;
            
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