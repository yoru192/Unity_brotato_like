using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BerserkPixel.Tilemap_Generator.Extensions
{
    public static class TilemapExt
    {
        public static List<Vector3> GetWorldPositionsWithTiles(this Tilemap tilemap)
        {
            List<Vector3> tileWorldLocations = new List<Vector3>();
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {   
                Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
                if (tilemap.HasTile(localPlace))
                {
                    Vector3 place = tilemap.CellToWorld(localPlace);
                    tileWorldLocations.Add(place);
                }
            }

            return tileWorldLocations;
        }
        
        public static List<Vector3> GetUsedTilesInAllTilemaps(this Tilemap[] tilemaps)
        {
            List<Vector3> allUsedTiles = new List<Vector3>();

            foreach (var tilemap in tilemaps)
            {
                allUsedTiles.AddRange(tilemap.GetWorldPositionsWithTiles());
            }
            
            return allUsedTiles;
        }
        
        public static List<Vector3> GetWorldPositionsForTile(this Tilemap tilemap, TileBase tile)
        {
            List<Vector3> tileWorldLocations = new List<Vector3>();
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {   
                Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
                if (tilemap.HasTile(localPlace) && tilemap.GetTile(localPlace) == tile)
                {
                    Vector3 place = tilemap.CellToWorld(localPlace);
                    tileWorldLocations.Add(place);
                }
            }

            return tileWorldLocations;
        }
        
        public static List<Vector3Int> GetLocalPositionsForTile(this Tilemap tilemap, TileBase tile)
        {
            List<Vector3Int> tileWorldLocations = new List<Vector3Int>();
            foreach (var pos in tilemap.cellBounds.allPositionsWithin)
            {   
                Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
                if (tilemap.HasTile(localPlace) && tilemap.GetTile(localPlace) == tile)
                {
                    tileWorldLocations.Add(localPlace);
                }
            }

            return tileWorldLocations;
        }

        public static bool IsPositionTypeOfTile(this Tilemap tilemap, TileBase tile, Vector3 worldPosition)
        {
            Vector3Int localPlace = tilemap.WorldToCell(worldPosition);
            return tilemap.HasTile(localPlace) && tilemap.GetTile(localPlace) == tile;
        }

        public static TileHitInfo<T> GetTileFromWorldPosition2D<T>(this Tilemap tilemap, Vector2 worldPosition) where T : TileBase
        {
            Vector3Int localPlace = tilemap.WorldToCell(worldPosition);
            DebugDrawRect(localPlace, 1, Color.magenta, 1);
            
            var value = new TileHitInfo<T>
            {
                tile = tilemap.GetTile(localPlace) as T,
                position = localPlace
            };
            return value;
        }

        public static T GetTileFromWorldPosition2D<T>(this Tilemap tilemap, Vector3Int localPlace) where T : TileBase
        {
            return tilemap.GetTile(localPlace) as T;
        }

        public static void RefreshTileFromWorldPosition(this Tilemap tilemap, Vector3Int localPlace)
        {
            DebugDrawRect(localPlace, 1, Color.red, 2);
            
            // this will call the Tile.GetTileData
            tilemap.RefreshTile(localPlace);
            // this makes sure they are rendered properly
            tilemap.SetTile(localPlace, null);
        }

        public static void DebugDrawRect(Vector3 position, float size, Color color, float duration)
        {
            Debug.DrawLine(position, new Vector3(position.x + size, position.y, position.z), color, duration);
            Debug.DrawLine(position, new Vector3(position.x, position.y - size, position.z), color, duration);
            Debug.DrawLine(new Vector3(position.x, position.y - size, position.z), new Vector3(position.x + size, position.y - size, position.z), color, duration);
            Debug.DrawLine(new Vector3(position.x + size, position.y - size, position.z), new Vector3(position.x + size, position.y, position.z), color, duration);
        }
    }

    public struct TileHitInfo<T>
    {
        public T tile;
        public Vector3Int position;
    }
}