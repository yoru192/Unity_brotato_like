using System.Collections.Generic;

namespace CodeBase.Infrastructure.Map
{
    /// <summary>
    /// A single point on the generated map. One node == one run (level) with its own
    /// wave configuration, enemies and difficulty. Edges go upward (row -> row + 1).
    /// </summary>
    public class MapNode
    {
        public readonly int Row;
        public readonly int Column;

        public NodeType Type;
        public NodeState State = NodeState.Locked;

        /// <summary>Stable id within the map, used for save/restore of the chosen path.</summary>
        public int Id;

        public readonly List<MapNode> Outgoing = new List<MapNode>();
        public readonly List<MapNode> Incoming = new List<MapNode>();

        public MapNode(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public bool IsBoss => Type == NodeType.Boss;
    }
}
