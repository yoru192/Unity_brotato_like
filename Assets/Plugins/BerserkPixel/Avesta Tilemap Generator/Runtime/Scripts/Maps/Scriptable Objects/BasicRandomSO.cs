using UnityEngine;

namespace BerserkPixel.Tilemap_Generator.SO
{
    [CreateAssetMenu(fileName = "New Map Configuration", menuName = "Avesta/Maps/Basic Random")]
    public class BasicRandomSO : MapConfigSO
    {
        public float fillPercent;
        public string seed;
        public bool useRandomSeed = true;
    }
}