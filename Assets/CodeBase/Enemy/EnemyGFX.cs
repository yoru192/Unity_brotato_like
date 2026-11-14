using Pathfinding;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyGFX : MonoBehaviour
    {
        public FollowerEntity followerEntity;

        [Header("Facing")]
        [Tooltip("Ближче за цю відстань до цілі ворог дивиться на ціль, а не за напрямком руху.")]
        [SerializeField] private float faceTargetDistance = 3f;
        [Tooltip("Мінімальна різниця по X до цілі, щоб розвернутися (щоб не смикатись, коли ціль прямо над/під ворогом).")]
        [SerializeField] private float targetDeadzoneX = 0.15f;
        [Tooltip("Мінімальна швидкість по X, щоб розвернутися за напрямком руху. Відсікає дрижання від зіткнень і обходу сусідів.")]
        [SerializeField] private float velocityThresholdX = 0.3f;

        private Vector3 _spriteScale;
        private int _facing = 1;

        public void Construct(Vector3 spriteScale)
        {
            _spriteScale = spriteScale;
        }

        void Start()
        {
            if (followerEntity == null)
            {
                followerEntity = GetComponent<FollowerEntity>();
            }
        }

        void Update()
        {
            // До Construct масштаб ще невідомий — не чіпаємо, щоб не занулити спрайт.
            if (_spriteScale == Vector3.zero) return;

            UpdateFacing();
            transform.localScale = new Vector3(_facing * _spriteScale.x, _spriteScale.y, _spriteScale.z);
        }

        private void UpdateFacing()
        {
            if (followerEntity == null) return;

            // Поруч із ціллю швидкість ненадійна: ворог штовхається з гравцем та сусідами,
            // і знак швидкості скаче. Тому зблизька дивимось просто на ціль.
            Vector3 toTarget = followerEntity.destination - followerEntity.position;
            bool hasTarget = !float.IsInfinity(toTarget.x) && !float.IsNaN(toTarget.x)
                             && !float.IsInfinity(toTarget.y) && !float.IsNaN(toTarget.y);

            if (hasTarget && ((Vector2)toTarget).sqrMagnitude <= faceTargetDistance * faceTargetDistance)
            {
                if (Mathf.Abs(toTarget.x) > targetDeadzoneX)
                    _facing = toTarget.x > 0f ? 1 : -1;
                return;
            }

            float velocityX = followerEntity.velocity.x;
            if (Mathf.Abs(velocityX) > velocityThresholdX)
                _facing = velocityX > 0f ? 1 : -1;
        }
    }
}
