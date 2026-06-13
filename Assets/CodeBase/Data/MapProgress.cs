using System;
using System.Collections.Generic;

namespace CodeBase.Data
{
    /// <summary>
    /// Persisted state of the procedural level map. The map itself is rebuilt from <see cref="seed"/>,
    /// so only the seed and the player's path through it are stored.
    /// </summary>
    [Serializable]
    public class MapProgress
    {
        public bool hasMap;
        public int version;
        public int seed;

        /// <summary>Node ids completed so far, in completion order. The last one defines the frontier.</summary>
        public List<int> completedNodeIds = new List<int>();

        /// <summary>Node currently selected and being played; -1 when the player is back at the map.</summary>
        public int currentNodeId = -1;
    }
}
