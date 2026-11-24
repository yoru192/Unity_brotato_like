using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace CodeBase.Logic.Arena
{
    /// <summary>
    /// Root of an arena layout prefab: playable bounds, hero start and enemy spawn points.
    /// Instantiated by the factory into the Arena scene; LoadLevelState reads the level data from it
    /// instead of a per-scene LevelStaticData.
    /// </summary>
    public class ArenaLayout : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(40f, 24f);
        [SerializeField] private Transform heroSpawn;
        [Tooltip("Every child transform is an enemy spawn point. Keep them on the arena edge.")]
        [SerializeField] private Transform spawnPointsRoot;

        public Rect Bounds => new Rect((Vector2)transform.position - size * 0.5f, size);

        public Vector3 HeroPosition => heroSpawn != null ? heroSpawn.position : transform.position;

        public List<Vector2> SpawnPositions()
        {
            var positions = new List<Vector2>();
            if (spawnPointsRoot == null)
                return positions;

            foreach (Transform point in spawnPointsRoot)
                positions.Add(point.position);
            return positions;
        }

        /// <summary>Fits the scene's grid graph to the arena bounds and rescans it (obstacles included).</summary>
        public void ScanNavigation()
        {
            AstarPath astar = AstarPath.active;
            if (astar == null)
            {
                Debug.LogError("ArenaLayout: no AstarPath in the scene");
                return;
            }

            GridGraph grid = astar.data.gridGraph;
            if (grid != null)
            {
                Rect bounds = Bounds;
                grid.center = new Vector3(bounds.center.x, bounds.center.y, 0f);
                grid.SetDimensions(Mathf.CeilToInt(bounds.width / grid.nodeSize),
                    Mathf.CeilToInt(bounds.height / grid.nodeSize), grid.nodeSize);
            }

            astar.Scan();
        }

        private void OnDrawGizmos()
        {
            Rect bounds = Bounds;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            if (heroSpawn != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(heroSpawn.position, 0.5f);
            }

            if (spawnPointsRoot != null)
            {
                Gizmos.color = Color.red;
                foreach (Transform point in spawnPointsRoot)
                    Gizmos.DrawWireSphere(point.position, 0.4f);
            }
        }
    }
}
