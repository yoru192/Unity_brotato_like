using CodeBase.Common;
using CodeBase.Infrastructure.Services.ProgressService;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Loot
{
    public class XpOrb : MonoBehaviour, IPoolable
    {
        [SerializeField] private float attractRadius = 3f;
        [SerializeField] private float attractSpeed = 8f;
        [SerializeField] private float attractAcceleration = 5f;
        [SerializeField] private float attractMaxSpeed = 20f;

        private int _xpAmount;
        private IProgressService _progressService;
        private Transform _playerTransform;
        private bool _isAttracting;
        private float _currentSpeed;

        public void Construct(int xpAmount, IProgressService progressService, Transform playerTransform)
        {
            _xpAmount = xpAmount;
            _progressService = progressService;
            _playerTransform = playerTransform;
        }

        public void OnSpawn() { }

        public void OnDespawn()
        {
            _xpAmount = 0;
            _progressService = null;
            _playerTransform = null;
            _isAttracting = false;
            _currentSpeed = 0f;
        }

        private void Update()
        {
            if (_playerTransform == null) return;

            if (!_isAttracting)
            {
                float dist = Vector2.Distance(transform.position, _playerTransform.position);
                if (dist <= attractRadius)
                {
                    _isAttracting = true;
                    _currentSpeed = attractSpeed;
                }
            }

            if (_isAttracting)
            {
                _currentSpeed = Mathf.Min(_currentSpeed + attractAcceleration * Time.deltaTime, attractMaxSpeed);
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    _playerTransform.position,
                    _currentSpeed * Time.deltaTime
                );
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == PhysicsLayers.Player)
            {
                _progressService.AddXp(_xpAmount);
                ObjectPoolManager.ReturnObjectToPool(gameObject, ObjectPoolManager.PoolType.XpOrb);
            }
        }
    }
}
