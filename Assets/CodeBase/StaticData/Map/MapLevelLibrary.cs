using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Infrastructure.Map;
using UnityEngine;

namespace CodeBase.StaticData.Map
{
    /// <summary>
    /// Content for the procedural map: a pool of wave configurations per node type. The wave
    /// config for a given node is chosen deterministically (map seed + node id) so the same node
    /// always launches the same run.
    /// </summary>
    [CreateAssetMenu(fileName = "MapLevelLibrary", menuName = "StaticData/Map/Level Library")]
    public class MapLevelLibrary : ScriptableObject
    {
        public List<TypedLevelPool> pools = new List<TypedLevelPool>();

        [Serializable]
        public class TypedLevelPool
        {
            public NodeType type;
            public List<WaveControllerStaticData> configs = new List<WaveControllerStaticData>();
        }

        public WaveControllerStaticData WaveConfigFor(MapNode node, int mapSeed)
        {
            List<WaveControllerStaticData> pool = PoolFor(node.Type);
            if (pool == null || pool.Count == 0)
            {
                Debug.LogError($"MapLevelLibrary has no wave configs for node type {node.Type}");
                return null;
            }

            // Deterministic per node: same map + same node => same config, but varied across the map.
            int index = new System.Random(mapSeed * 73856093 ^ node.Id * 19349663).Next(pool.Count);
            return pool[index];
        }

        private List<WaveControllerStaticData> PoolFor(NodeType type) =>
            pools.FirstOrDefault(p => p.type == type)?.configs;
    }
}
