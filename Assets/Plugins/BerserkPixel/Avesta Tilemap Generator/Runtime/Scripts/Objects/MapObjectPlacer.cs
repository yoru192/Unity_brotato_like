using System.Collections.Generic;
using System.Linq;
using BerserkPixel.Tilemap_Generator.Extensions;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace BerserkPixel.Tilemap_Generator
{
    public class MapObjectPlacer : MonoBehaviour
    {
        [SerializeField] private Tilemap targetTilemap;
        [SerializeField] private Tilemap[] obstaclesTilemaps;
        
        [Tooltip("True will update inside the Editor whenever a new change is made on this script")]
        public bool autoUpdate = true;
        
        [Header("Objects To Place")]
        [SerializeField] private TileObjects[] objects;
        [SerializeField, Range(0, 100), Delayed] private int objectFillPercent = 3;
        
        [Header("Clustering")]
        [SerializeField] private bool useClusters = true;
        [SerializeField, Delayed] private int maxObjectsPerCluster = 4;
        [SerializeField, Range(0f, 10f), Delayed] private float spacing = 2f;

        private readonly Dictionary<Vector3, GameObject> _terrainObjects = new Dictionary<Vector3, GameObject>();

        private Transform _mapObjects;
        private WeightedRandomPicker<GameObject> _weightedRandomPicker;

        private List<Vector3> _bannedPlaces = new List<Vector3>();

        private void Init()
        {
            if (targetTilemap == null)
            {
                Debug.LogError("You need to set a Level Generator on the inspector");
            }
        }

        #region Editor

        public void PlaceObjects()
        {
            Init();
            
            if (targetTilemap == null) return;
            
            if (_bannedPlaces.Count <= 0)
            {
                _bannedPlaces = obstaclesTilemaps.GetUsedTilesInAllTilemaps();
            }

            var terrainTiles = targetTilemap.GetWorldPositionsWithTiles();

            var hasAnyPrefab = objects.Any(o => o.prefab != null);
            
            if (!hasAnyPrefab || terrainTiles.Count <= 0)
            {
                Debug.LogError($"There are no objects with prefabs or the Tilemap is empty. " +
                               $"Check your objects array and the tile you are searching for.");
                return;
            }
            
            var candidates = objects.Select(o => o.prefab).ToList();
            var weights = objects.Select(o => o.weight).ToList();
            _weightedRandomPicker = new WeightedRandomPicker<GameObject>(candidates, weights);
            
            _mapObjects = GetMapObjectsTransform();

            foreach (var worldPosition in terrainTiles)
            {
                bool shouldPlaceObject = Random.Range(0f, 100f) < objectFillPercent;

                // if the chance is not enough we continue
                if (!shouldPlaceObject) continue;

                var isBannedPlace = _bannedPlaces.Contains(worldPosition);
                // if we are not allowed to place a tile there (can be a wall or used
                if (isBannedPlace) continue;

                if (useClusters)
                {
                    PlaceClusterObjects(_weightedRandomPicker.Pick(), worldPosition);
                }
                else
                {
                    PlaceUniqueObject(_weightedRandomPicker.Pick(), worldPosition);
                }
            }            
        }

        private void PlaceUniqueObject(GameObject prefab, Vector3 position)
        {
            // if we already have an object on that position
            if (_terrainObjects.ContainsKey(position)) return;
            
            var objectInTilemap = Instantiate(prefab, _mapObjects.transform);
            objectInTilemap.transform.position = position;
            _terrainObjects.Add(position, objectInTilemap);
        }

        private void PlaceClusterObjects(GameObject prefab, Vector3 position)
        {
            // we have already decided there's at least one object here
            var amount = Random.Range(1, maxObjectsPerCluster);

            var initialPosition = new Vector2(position.x, position.y);
            
            for (var i = 0; i < amount; i++)
            {
                var delta = Random.insideUnitCircle * spacing;
                var point = initialPosition + delta;
                
                var isBannedPlace = _bannedPlaces.Contains(point);
                // if we are not allowed to place a tile there (can be a wall or used
                if (isBannedPlace) continue;

                // if we already have an object on that position
                if (_terrainObjects.ContainsKey(point))
                    continue;
                
                var objectInTilemap = Instantiate(prefab, _mapObjects.transform);
                objectInTilemap.transform.position = point;
                _terrainObjects.Add(point, objectInTilemap);
            }
        }

        private Transform GetMapObjectsTransform()
        {
            var currentObjects = targetTilemap.transform.Find("Objects");
            if (currentObjects != null)
            {
                return currentObjects;
            }

            return new GameObject("Objects")
            {
                transform =
                {
                    parent = targetTilemap.transform
                }
            }.transform;
        }

        public void ClearObjects()
        {
            Init();
            
            _terrainObjects.Clear();
            _bannedPlaces.Clear();
            
            if (targetTilemap == null) return;
            
            var currentMapObjects = GetMapObjectsTransform();
            if (currentMapObjects != null)
                DestroyImmediate(currentMapObjects.gameObject);
        }

        #endregion
    }
}