using Pathfinding;
using UnityEngine;

namespace CodeBase.Logic.Arena
{
    /// <summary>
    /// Root of an arena layout prefab: playable bounds and hero start. Instantiated by the factory
    /// into the Arena scene; LoadLevelState reads the level data from it instead of a per-scene
    /// LevelStaticData. Enemies spawn just off-screen around the hero (EnemySpawner), so the
    /// layout has no fixed spawn points.
    /// </summary>
    public class ArenaLayout : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(324f, 180f);
        [SerializeField] private Transform heroSpawn;
        [SerializeField] private ArenaGround ground;

        public Rect Bounds => new Rect((Vector2)transform.position - size * 0.5f, size);

        public Vector3 HeroPosition => heroSpawn != null ? heroSpawn.position : transform.position;

        /// <summary>Builds the ground and fits + rescans the scene's grid graph to the arena bounds.</summary>
        public void Build()
        {
            if (ground != null)
                ground.Fill(Bounds);

            ScanNavigation();
        }

        private void ScanNavigation()
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
        }
    }
}
