using UnityEngine;

namespace BerserkPixel.Tilemap_Generator.SO
{
    [CreateAssetMenu(fileName = "New Map Configuration", menuName = "Avesta/Maps/Cellular")]
    public class CellularMapConfigSO : MapConfigSO
    {
        public float fillPercent;
        public string seed;
        public bool useRandomSeed = true;
        [Range(0, 10)] public int smoothSteps = 5;
        [Range(0, 10)] public int smoothThreshold = 4;
    }
}