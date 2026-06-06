using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeBase.Infrastructure.Map
{
    /// <summary>
    /// Procedural branching-map generator in the Slay the Spire style. Pure C# (System.Random),
    /// fully deterministic for a given seed. Walks several start->boss paths through a grid;
    /// cells that get visited become real nodes and the steps between them become edges.
    /// </summary>
    public class MapGenerator
    {
        private readonly MapGenerationConfig _config;

        public MapGenerator(MapGenerationConfig config)
        {
            _config = config;
        }

        public GeneratedMap Generate(int seed)
        {
            Random random = new Random(seed);

            // grid[row][col] is null until a path visits that cell.
            MapNode[][] grid = new MapNode[_config.Rows][];
            for (int row = 0; row < _config.Rows; row++)
                grid[row] = new MapNode[_config.Columns];

            // Edges already drawn out of each row, used to forbid crossings.
            List<List<(int from, int to)>> edgesPerRow = new List<List<(int, int)>>();
            for (int row = 0; row < _config.Rows; row++)
                edgesPerRow.Add(new List<(int, int)>());

            for (int path = 0; path < _config.PathCount; path++)
                BuildPath(grid, edgesPerRow, random);

            List<List<MapNode>> nodesByRow = CollectNodes(grid);
            AssignIds(nodesByRow);
            AssignTypes(nodesByRow, random);
            MarkEntrancesAvailable(nodesByRow);

            return new GeneratedMap(seed, _config.Rows, _config.Columns, nodesByRow);
        }

        private void BuildPath(MapNode[][] grid, List<List<(int from, int to)>> edgesPerRow, Random random)
        {
            int column = random.Next(_config.Columns);
            MapNode current = GetOrCreate(grid, 0, column);

            for (int row = 0; row < _config.Rows - 1; row++)
            {
                int nextColumn = ChooseNextColumn(column, edgesPerRow[row], random);
                MapNode next = GetOrCreate(grid, row + 1, nextColumn);

                Connect(current, next);
                edgesPerRow[row].Add((column, nextColumn));

                current = next;
                column = nextColumn;
            }
        }

        private int ChooseNextColumn(int column, List<(int from, int to)> rowEdges, Random random)
        {
            List<int> candidates = new List<int>();
            foreach (int delta in new[] { -1, 0, 1 })
            {
                int candidate = column + delta;
                if (candidate >= 0 && candidate < _config.Columns)
                    candidates.Add(candidate);
            }

            Shuffle(candidates, random);

            foreach (int candidate in candidates)
            {
                if (!CrossesExistingEdge(column, candidate, rowEdges))
                    return candidate;
            }

            // Every diagonal would cross — going straight up never crosses another edge.
            return column;
        }

        private static bool CrossesExistingEdge(int from, int to, List<(int from, int to)> rowEdges)
        {
            foreach ((int otherFrom, int otherTo) in rowEdges)
            {
                bool crosses = (from < otherFrom && to > otherTo) ||
                               (from > otherFrom && to < otherTo);
                if (crosses)
                    return true;
            }

            return false;
        }

        private static MapNode GetOrCreate(MapNode[][] grid, int row, int column)
        {
            return grid[row][column] ?? (grid[row][column] = new MapNode(row, column));
        }

        private static void Connect(MapNode from, MapNode to)
        {
            if (from.Outgoing.Contains(to))
                return;

            from.Outgoing.Add(to);
            to.Incoming.Add(from);
        }

        private static List<List<MapNode>> CollectNodes(MapNode[][] grid)
        {
            List<List<MapNode>> nodesByRow = new List<List<MapNode>>();
            foreach (MapNode[] row in grid)
            {
                List<MapNode> rowNodes = row.Where(node => node != null)
                    .OrderBy(node => node.Column)
                    .ToList();
                nodesByRow.Add(rowNodes);
            }

            return nodesByRow;
        }

        private static void AssignIds(List<List<MapNode>> nodesByRow)
        {
            int id = 0;
            foreach (List<MapNode> row in nodesByRow)
            foreach (MapNode node in row)
                node.Id = id++;
        }

        private void AssignTypes(List<List<MapNode>> nodesByRow, Random random)
        {
            int lastRow = nodesByRow.Count - 1;

            for (int row = 0; row < nodesByRow.Count; row++)
            {
                foreach (MapNode node in nodesByRow[row])
                {
                    if (row == lastRow)
                        node.Type = NodeType.Boss;
                    else if (row == 0)
                        node.Type = NodeType.Combat;
                    else
                        node.Type = RollMiddleType(node, row, random);
                }
            }
        }

        private NodeType RollMiddleType(MapNode node, int row, Random random)
        {
            bool parentIsShop = node.Incoming.Any(parent => parent.Type == NodeType.Shop);

            double roll = random.NextDouble();

            if (!parentIsShop && roll < _config.ShopChance)
                return NodeType.Shop;

            if (row >= _config.MinEliteRow && roll < _config.ShopChance + _config.EliteChance)
                return NodeType.Elite;

            if (roll < _config.ShopChance + _config.EliteChance + _config.CampfireChance)
                return NodeType.Campfire;

            return NodeType.Combat;
        }

        private static void MarkEntrancesAvailable(List<List<MapNode>> nodesByRow)
        {
            if (nodesByRow.Count == 0)
                return;

            foreach (MapNode entrance in nodesByRow[0])
                entrance.State = NodeState.Available;
        }

        private static void Shuffle<T>(IList<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
