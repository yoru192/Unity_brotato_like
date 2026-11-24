using UnityEngine;
using UnityEngine.Tilemaps;

namespace CodeBase.Logic.Arena
{
    /// <summary>
    /// Fills the arena ground tilemap at load time, so a big arena does not store every cell in the prefab.
    /// </summary>
    public class ArenaGround : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private TileBase groundTile;
        [Tooltip("Extra cells around the arena bounds, so the screen edge never shows empty space.")]
        [SerializeField] private int margin = 2;

        public void Fill(Rect bounds)
        {
            if (tilemap == null || groundTile == null)
            {
                Debug.LogError("ArenaGround: tilemap or ground tile is not set");
                return;
            }

            int xMin = Mathf.FloorToInt(bounds.xMin) - margin;
            int yMin = Mathf.FloorToInt(bounds.yMin) - margin;
            int width = Mathf.CeilToInt(bounds.xMax) + margin - xMin;
            int height = Mathf.CeilToInt(bounds.yMax) + margin - yMin;

            var tiles = new TileBase[width * height];
            for (int i = 0; i < tiles.Length; i++)
                tiles[i] = groundTile;

            tilemap.ClearAllTiles();
            tilemap.SetTilesBlock(new BoundsInt(xMin, yMin, 0, width, height, 1), tiles);
        }
    }
}
