using UnityEngine;
using UnityEngine.Tilemaps;

namespace BerserkPixel.Tilemap_Generator
{
    public class MapRenderer : MonoBehaviour
    {
        [Tooltip("Add your Ground Tile here")]
        public TileBase topTile;
        [Tooltip("Add your Background/Water Tile here")]
        public TileBase botTile;

        public void RenderMap(Tilemap tilemap, int width, int height, int[,] terrainMap)
        {
            Vector3Int[] positions = new Vector3Int[width * height];
            TileBase[] tileArray = new TileBase[positions.Length];

            int i = 0;
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    positions[i] = new Vector3Int(-x + width / 2, -y + height / 2, 0);
                    tileArray[i] = terrainMap[x, y] == MapGeneratorConst.TERRAIN_TILE ? topTile : botTile;
                    i++;
                }
            }
            
            tilemap.SetTiles(positions, tileArray);
        }

        public void ClearMap(Tilemap tilemap)
        {
            if (tilemap != null)
                tilemap.ClearAllTiles();
        }
    }
}