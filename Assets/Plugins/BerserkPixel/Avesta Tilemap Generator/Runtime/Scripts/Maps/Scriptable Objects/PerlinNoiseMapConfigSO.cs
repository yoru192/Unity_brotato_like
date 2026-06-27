using UnityEngine;

namespace BerserkPixel.Tilemap_Generator.SO
{
    [CreateAssetMenu(fileName = "New Map Configuration", menuName = "Avesta/Maps/Perlin")]
    public class PerlinNoiseMapConfigSO : MapConfigSO
    {
        public float fillPercent;
        public bool isIsland = true;
        public float islandSizeFactor = 4.2f;
        public float scale = 20f;
        public float offsetX = 100f;
        public float offsetY = 100f;
    }
}