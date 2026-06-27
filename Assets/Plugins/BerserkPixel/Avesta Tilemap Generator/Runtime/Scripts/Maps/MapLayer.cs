using System;
using System.Collections.Generic;
using BerserkPixel.Tilemap_Generator.Algorithms;
using BerserkPixel.Tilemap_Generator.Extensions;
using BerserkPixel.Tilemap_Generator.SO;
using BerserkPixel.Tilemap_Generator.Utilities;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace BerserkPixel.Tilemap_Generator
{
    [Serializable]
    public class MapLayer: ISerializationCallbackReceiver
    {
        [HideInInspector]
        public string name;
        
        [Tooltip("whether this layer should be taken into account when generating the final grid")]
        public bool active = true;

        [Header("Tilemap")] 
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private bool generateColliders = true;
        [Tooltip("Add your Tile here")] 
        [SerializeField] private TileBase tile;
        
        [Header("Algorithm")]
        [Expandable(BackgroundStyles.Darken)]
        [SerializeField] private MapConfigSO mapConfig;

        public Tilemap TileMap => tilemap;

        public List<Vector3> UsedTiles => _usedTiles;

        private List<Vector3> _usedTiles = new List<Vector3>();
        private int[,] _terrainMap;
        private IMapAlgorithm _mapAlgorithm;
        private MapType _mapType;

        private bool Init()
        {
            _mapAlgorithm = _mapType.GetFromType(mapConfig);
            return _mapAlgorithm != null;
        }

        /// <summary>
        /// It's in charge of generating the map. Must be overwritten by the Algorithm
        /// </summary>
        public void GenerateMap()
        {
            if (active && Init())
            {
                ClearMap();

                GenerateColliders();

                _terrainMap = _mapAlgorithm.RandomFillMap();

                PlaceTiles();
            }
        }

        private string GetMapName() => _mapAlgorithm.GetMapName();

        /// <summary>
        /// Just forwards the rendering to the MapRenderer script
        /// </summary>
        private void PlaceTiles()
        {
            Init();
            RenderMap(mapConfig.width, mapConfig.height, _terrainMap);
        }

        /// <summary>
        /// If selected in the inspector, this method attaches a TilemapCollider2D and a
        /// Rigidbody2D properly to the Tilemap
        /// </summary>
        private void GenerateColliders()
        {
            if (!generateColliders) return;

            var tilemapCollider2D = tilemap.gameObject.GetComponent<TilemapCollider2D>();
            if (tilemapCollider2D != null)
            {
                tilemapCollider2D.ProcessTilemapChanges();
                return;
            }

            var tilemapCollider = tilemap.gameObject.AddComponent<TilemapCollider2D>();
            tilemap.gameObject.AddComponent<CompositeCollider2D>();

            var rb = tilemap.gameObject.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            tilemapCollider.usedByComposite = true;
        }

        /// <summary>
        /// Used by the MapObjectPlacer script to obtain all the world positions where there's a
        /// Terrain tile
        /// </summary>
        /// <returns>A list of all the positions in the World (not Tilemap coordinates) where there's a Terrain tile</returns>
        public List<Vector3> GetTerrainTilesPositions() => TileMap.GetWorldPositionsForTile(tile);

        /// <summary>
        /// Checks if a certain world position is a Terrain tile
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns>True if the given position is a Terrain tile, False otherwise</returns>
        public bool TerrainContainsPosition(Vector3 worldPosition) =>
            TileMap.IsPositionTypeOfTile(tile, worldPosition);

        /// <summary>
        /// Clears the tilemap and it's colliders
        /// </summary>
        public void ClearMap()
        {
            Init();

            if (tilemap != null)
            {
                tilemap.ClearAllTiles();
                tilemap.RefreshAllTiles();
            }
            
            _usedTiles.Clear();

            if (!generateColliders) return;

            if (tilemap.gameObject.TryGetComponent(out TilemapCollider2D tilemapCollider2D))
                Object.DestroyImmediate(tilemapCollider2D);
            
            if (tilemap.gameObject.TryGetComponent(out CompositeCollider2D compositeCollider2D))
                Object.DestroyImmediate(compositeCollider2D);
            
            if (tilemap.gameObject.TryGetComponent(out Rigidbody2D rb)) Object.DestroyImmediate(rb);
        }

        private void RenderMap(int width, int height, int[,] terrainMap)
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (terrainMap[x, y] == MapGeneratorConst.TERRAIN_TILE)
                    {
                        var position = new Vector3Int(-x + width / 2, -y + height / 2, 0);
                        tilemap.SetTile(position, tile);
                        
                        _usedTiles.Add(tilemap.CellToWorld(position));
                    }
                }
            }
            
            tilemap.RefreshAllTiles();
        }

        public void OnBeforeSerialize()
        {
            if (mapConfig == null) return;
            
            _mapType = mapConfig.GetFromSO();

            name = GenerateName();
        }

        public void OnAfterDeserialize()
        {
            if (mapConfig == null) return;
            
            _mapType = mapConfig.GetFromSO();

            name = GenerateName();
        }

        private string GenerateName()
        {
            var activeStatus = active ? "[ACTIVE]" : "[INACTIVE]";
            return $"{SplitCamelCase(_mapType.ToString())} {activeStatus}";
        }
        
        private string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }
    }
}