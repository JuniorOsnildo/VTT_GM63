using UnityEngine;
using System.Collections.Generic;
using Core;
using VTT.Unity;



namespace VTT.Unity
{
    public class PathRenderer
    {
        private List<GameObject> pathVisuals = new List<GameObject>();
        private GridManager gridManager;
        
        public PathRenderer(GridManager gridManager)
        {
            this.gridManager = gridManager;
        }

        public void DrawPath(List<GridCoordinate> path, Color pathColor)
        {
            foreach (var coord in path)
            {
                GameObject pathMarker =
                    GameObject.CreatePrimitive(PrimitiveType.Cube);

                pathMarker.name =
                    $"PathMarker_{coord.X}_{coord.Y}";

                Collider col =
                    pathMarker.GetComponent<Collider>();

                if (col != null)
                    Object.Destroy(col);

                Renderer renderer =
                    pathMarker.GetComponent<Renderer>();

                renderer.material = new Material(
                    gridManager.GetGridMaterial()
                );

                renderer.material.color = pathColor;

                Vector3 worldPos =
                    gridManager.GridCoordToWorldPosition(coord);

                pathMarker.transform.position =
                    new Vector3(
                        worldPos.x,
                        0.15f,
                        worldPos.z
                    );

                pathMarker.transform.localScale =
                    new Vector3(0.8f, 0.05f, 0.8f);

                pathVisuals.Add(pathMarker);
            }
        }
        
        public void ClearPath()
        {
            foreach (var visual in pathVisuals)
            {
                Object.Destroy(visual);
            }
            pathVisuals.Clear();
        }
    }
}