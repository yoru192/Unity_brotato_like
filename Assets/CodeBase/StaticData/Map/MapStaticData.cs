using CodeBase.Infrastructure.Map;
using UnityEngine;

namespace CodeBase.StaticData.Map
{
    /// <summary>
    /// Editor-tunable parameters for procedural map generation. Mirrors
    /// <see cref="MapGenerationConfig"/> and converts to it for the pure generator.
    /// </summary>
    [CreateAssetMenu(fileName = "MapStaticData", menuName = "StaticData/Map/Map Settings")]
    public class MapStaticData : ScriptableObject
    {
        [Header("Grid")]
        [Min(2)] public int rows = 12;
        [Min(2)] public int columns = 7;

        [Header("Paths")]
        [Min(1)] public int pathCount = 6;

        [Header("Node Type Chances")]
        [Range(0f, 1f)] public float eliteChance = 0.16f;
        [Range(0f, 1f)] public float shopChance = 0.12f;
        [Range(0f, 1f)] public float campfireChance = 0.10f;
        [Min(0)] public int minEliteRow = 3;

        public MapGenerationConfig ToGenerationConfig() => new MapGenerationConfig
        {
            Rows = rows,
            Columns = columns,
            PathCount = pathCount,
            EliteChance = eliteChance,
            ShopChance = shopChance,
            CampfireChance = campfireChance,
            MinEliteRow = minEliteRow,
        };
    }
}
