using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeBase.Infrastructure.Map
{
    /// <summary>
    /// Procedural branching-map generator in the Slay the Spire style. Pure C# (System.Random),
    /// fully deterministic for a given seed. Walks several start->pre-boss paths through a grid,
    /// then converges them into one final boss node.
    /// </summary>
    public class MapGenerator
    {
        private readonly MapGenerationConfig _config;

        private int Rows => Math.Max(3, _config.Rows);
        private int Columns => Math.Max(3, _config.Columns);
        private int PathCount => Math.Max(1, _config.PathCount);
        private int BossColumn => Columns / 2;
        private int BossRow => Rows - 1;
        private int PreBossRow => Rows - 2;

        public MapGenerator(MapGenerationConfig config)
        {
            _config = config;
        }

        public GeneratedMap Generate(int seed)
        {
            Random random = new Random(seed);

            // grid[row][col] is null until a path visits that cell.
            MapNode[][] grid = new MapNode[Rows][];
            for (int row = 0; row < Rows; row++)
                grid[row] = new MapNode[Columns];

            // Edges already drawn out of each row, used to forbid crossings.
            List<List<(int from, int to)>> edgesPerRow = new List<List<(int, int)>>();
            for (int row = 0; row < Rows; row++)
                edgesPerRow.Add(new List<(int, int)>());

            for (int path = 0; path < PathCount; path++)
                BuildPath(grid, edgesPerRow, random);

            ConnectPreBossNodesToSingleBoss(grid, edgesPerRow);

            List<List<MapNode>> nodesByRow = CollectNodes(grid);
            AssignIds(nodesByRow);
            AssignTypes(nodesByRow, random);
            MarkEntrancesAvailable(nodesByRow);

            return new GeneratedMap(seed, Rows, Columns, nodesByRow);
        }

        private void BuildPath(MapNode[][] grid, List<List<(int from, int to)>> edgesPerRow, Random random)
        {
            int column = random.Next(Columns);
            MapNode current = GetOrCreate(grid, 0, column);

            // Walk only to the row before the boss. The final row is one shared boss node.
            for (int row = 0; row < PreBossRow; row++)
            {
                int nextColumn = ChooseNextColumn(column, edgesPerRow[row], random);
                MapNode next = GetOrCreate(grid, row + 1, nextColumn);

                Connect(current, next);
                edgesPerRow[row].Add((column, nextColumn));

                current = next;
                column = nextColumn;
            }
        }

        private void ConnectPreBossNodesToSingleBoss(MapNode[][] grid, List<List<(int from, int to)>> edgesPerRow)
        {
            MapNode boss = GetOrCreate(grid, BossRow, BossColumn);

            for (int column = 0; column < Columns; column++)
            {
                MapNode preBossNode = grid[PreBossRow][column];
                if (preBossNode == null)
                    continue;

                Connect(preBossNode, boss);
                edgesPerRow[PreBossRow].Add((column, BossColumn));
            }
        }

        private int ChooseNextColumn(int column, List<(int from, int to)> rowEdges, Random random)
        {
            List<int> candidates = new List<int>();
            foreach (int delta in new[] { -1, 0, 1 })
            {
                int candidate = column + delta;
                if (candidate >= 0 && candidate < Columns)
                    candidates.Add(candidate);
            }

            Shuffle(candidates, random);

            foreach (int candidate in candidates)
            {
                if (!CrossesExistingEdge(column, candidate, rowEdges))
                    return candidate;
            }

            // Every diagonal would cross, while going straight up never crosses another edge.
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
            for (int row = 0; row < nodesByRow.Count; row++)
            {
                foreach (MapNode node in nodesByRow[row])
                {
                    if (row == BossRow)
                        node.Type = NodeType.Boss;
                    else if (_config.ForceCampfireBeforeBoss && row == PreBossRow)
                        node.Type = NodeType.Campfire;
                    else if (row < _config.GuaranteedCombatRows)
                        node.Type = NodeType.Combat;
                    else
                        node.Type = RollMiddleType(node, row, random);
                }
            }
        }

        private NodeType RollMiddleType(MapNode node, int row, Random random)
        {
            bool parentIsShop = node.Incoming.Any(parent => parent.Type == NodeType.Shop);
            bool parentIsCampfire = node.Incoming.Any(parent => parent.Type == NodeType.Campfire);

            double roll = random.NextDouble();
            double threshold = 0f;

            if (!parentIsShop)
            {
                threshold += _config.ShopChance;
                if (roll < threshold)
                    return NodeType.Shop;
            }

            if (row >= _config.MinEliteRow)
            {
                threshold += _config.EliteChance;
                if (roll < threshold)
                    return NodeType.Elite;
            }

            if (!parentIsCampfire)
            {
                threshold += _config.CampfireChance;
                if (roll < threshold)
                    return NodeType.Campfire;
            }

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
