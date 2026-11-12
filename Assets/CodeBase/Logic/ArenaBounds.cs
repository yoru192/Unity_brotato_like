using UnityEngine;

namespace CodeBase.Logic
{
    public class ArenaBounds : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(60f, 34f);
        [SerializeField] private Vector2 offset = Vector2.zero;          // зміщення центру арени відносно цього об'єкта
        [SerializeField] private float wallThickness = 6f;               // товсті стіни не пропускають швидку ракету
        [SerializeField, Range(0f, 1f)] private float bounciness = 0.4f; // 0 = липне, 1 = повний відскок

        private void Awake() => BuildWalls();

        private void BuildWalls()
        {
            var material = new PhysicsMaterial2D("ArenaWall") { bounciness = bounciness, friction = 0f };

            float halfX = size.x * 0.5f;
            float halfY = size.y * 0.5f;
            float t = wallThickness;

            // Верх і низ ширші на товщину стіни, щоб кути були закриті
            CreateWall("Top",    offset + new Vector2(0f,  halfY + t * 0.5f), new Vector2(size.x + t * 2f, t), material);
            CreateWall("Bottom", offset + new Vector2(0f, -halfY - t * 0.5f), new Vector2(size.x + t * 2f, t), material);
            CreateWall("Left",   offset + new Vector2(-halfX - t * 0.5f, 0f), new Vector2(t, size.y), material);
            CreateWall("Right",  offset + new Vector2( halfX + t * 0.5f, 0f), new Vector2(t, size.y), material);
        }

        private void CreateWall(string wallName, Vector2 localPos, Vector2 wallSize, PhysicsMaterial2D material)
        {
            var wall = new GameObject(wallName);
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition = localPos;
            wall.layer = gameObject.layer;

            var box = wall.AddComponent<BoxCollider2D>();
            box.size = wallSize;
            box.sharedMaterial = material;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 center = transform.TransformPoint(offset);
            Gizmos.DrawWireCube(center, size);
        }
    }
}