using System.Collections.Generic;
using System.Linq;

namespace CodeBase.Infrastructure.Map
{
    /// <summary>
    /// A fully built branching map. Reconstructable from <see cref="Seed"/> alone, so only the
    /// seed (plus the chosen path) needs to be persisted in player progress.
    /// </summary>
    public class GeneratedMap
    {
        public readonly int Seed;
        public readonly int Rows;
        public readonly int Columns;

        /// <summary>Nodes grouped by row, bottom (0) to top (Rows - 1). Inner list is sorted by column.</summary>
        public readonly List<List<MapNode>> NodesByRow;

        public GeneratedMap(int seed, int rows, int columns, List<List<MapNode>> nodesByRow)
        {
            Seed = seed;
            Rows = rows;
            Columns = columns;
            NodesByRow = nodesByRow;
        }

        public IEnumerable<MapNode> AllNodes => NodesByRow.SelectMany(row => row);

        public IReadOnlyList<MapNode> Entrances => NodesByRow.Count > 0 ? NodesByRow[0] : new List<MapNode>();

        public MapNode NodeById(int id) => AllNodes.FirstOrDefault(node => node.Id == id);
    }
}
