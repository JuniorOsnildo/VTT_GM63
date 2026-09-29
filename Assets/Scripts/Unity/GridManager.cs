using System;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Session;

namespace VTT.Unity
{
    public class GridManager : MonoBehaviour
    {
        private int gridWidth;
        private int gridHeight;
        [SerializeField] private float cellSize = 1f;
        
        [SerializeField] private Material gridMaterial;
        [SerializeField] private Material tokenMaterial;
        
        public event Action OnMapLoaded;
        
        private TokenVisual playerToken;
        private BoardGrid boardGrid;
        
        private GameObject gridVisuals;
        private GameObject tokenVisuals;
        
        private Dictionary<string, TokenVisual> tokenVisualById = new Dictionary<string, TokenVisual>();
        
        private MapData currentMapData;
        
        public TokenVisual CreateTokenVisual(Token token)
        {
            GameObject tokenObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            tokenObj.name = $"Token_{token.Name}";

            Renderer renderer = tokenObj.GetComponent<Renderer>();

            renderer.material = new Material(tokenMaterial);
            renderer.material.color = SetFactionColor(token.Faction);

            tokenObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            if (tokenVisuals == null)
            {
                tokenVisuals = new GameObject("TokenVisuals");
                tokenVisuals.transform.parent = transform;
            }

            tokenObj.transform.parent = tokenVisuals.transform;

            TokenVisual visual = tokenObj.AddComponent<TokenVisual>();

            visual.Initialize(token, this);
            tokenVisualById[token.Id] = visual;
            
            if (boardGrid != null)
            {
                boardGrid.PlaceToken(token, token.Coordinates);
            }
            
            Physics.SyncTransforms();

            return visual;
        }
        
        public TokenVisual GetTokenVisual(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
                return null;

            if (tokenVisualById.TryGetValue(tokenId, out TokenVisual visual))
            {
                return visual;
            }

            return null;
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

                    renderer.material = new Material(gridMaterial);

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
        
        public void LoadMap(MapData mapData)
        {
            if (mapData == null)
            {
                Debug.LogWarning("[GRID] Tentativa de carregar um mapa nulo.");
                return;
            }

            if (gridVisuals != null)
            {
                Destroy(gridVisuals);
                gridVisuals = null;
            }

            if (tokenVisuals != null)
            {
                Destroy(tokenVisuals);
                tokenVisuals = null;
            }

            tokenVisualById.Clear();

            currentMapData = mapData;

            gridWidth = mapData.Width;
            gridHeight = mapData.Height;

            boardGrid = new BoardGrid(gridWidth, gridHeight);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    GridCoordinate coord = new GridCoordinate(x, y);
                    TerrainTag terrainTag = mapData.GetTerrainAt(coord);
                    TerrainType terrainType = TerrainDefinitions.Get(terrainTag).GameType;

                    boardGrid.SetCell(coord, terrainType);
                }
            }

            RenderGrid();
            OnMapLoaded?.Invoke();
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
            
            if (tokenVisuals != null)
            {
                Destroy(tokenVisuals);
                tokenVisuals = null;
            }
            
            tokenVisualById.Clear();
            
            boardGrid = room.Grid;
            currentMapData = room.MapData;
            
            gridWidth = boardGrid.width;
            gridHeight = boardGrid.height;
            
            RenderGrid();

            foreach (Token token in room.Tokens)
            {
                CreateTokenVisual(token);
            }
            
            OnMapLoaded?.Invoke();
        }
        
        public BoardGrid GetGrid() => boardGrid;
        
        public Material GetGridMaterial()
        {
            return gridMaterial;
        }
        
        public Vector3 GridCoordToWorldPosition(GridCoordinate coord)
            => new Vector3(coord.X * cellSize, 0, coord.Y * cellSize);
        
    }
}