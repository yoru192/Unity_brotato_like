using UnityEngine;

namespace BerserkPixel.Tilemap_Generator.SO
{
    [CreateAssetMenu(fileName = "New Map Configuration", menuName = "Avesta/Maps/Random Walk")]
    public class RandomWalkSO : MapConfigSO
    {
        public float fillPercent = 50;
        public string seed;
        public bool useRandomSeed = true;
        public bool invertGrid = false;
        public Vector2Int startingPoint = Vector2Int.zero;
        public int maxIterations = 1500;
    }
}