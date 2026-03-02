using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Weapon.RangeWeapon
{
    public enum ProjectileTrackingMode
    {
        /// <summary>
        /// Летить у точку де ціль була на момент пострілу.
        /// </summary>
        FixedPoint,

        /// <summary>
        /// Наводиться на ціль протягом homingDuration секунд, потім летить прямо.
        /// </summary>
        HomingThenStraight,

        /// <summary>
        /// Tabletop-style: розраховує де буде ціль через flightDuration секунд
        /// і стріляє туди. Гравець може відійти — але якщо рухається рівномірно,
        /// снаряд влучить точно. Відчуття "розумного" ворога.
        /// </summary>
        PredictiveShot,
    }
    
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private ProjectileVisual _projectileVisual;

        [Header("Arc")]
        [Tooltip("Висота дуги у world units. 1-2 для малих снарядів, 3-5 для навісних.")]
        [SerializeField] private float _arcHeight = 2f;

        [Header("Tracking")]
        [SerializeField] private ProjectileTrackingMode _trackingMode = ProjectileTrackingMode.FixedPoint;

        [Tooltip("Тільки для HomingThenStraight: скільки секунд снаряд наводиться на ціль.")]
        [SerializeField] private float _homingDuration = 0.4f;

        [Tooltip("Тільки для PredictiveShot: кількість ітерацій уточнення передбачення. " +
                 "2-3 достатньо, більше — точніше але дорожче.")]
        [SerializeField] private int _predictionIterations = 3;

        [Header("Hit")]
        [Tooltip("Радіус спрацювання колізії (по реальній позиції / тіні).")]
        [SerializeField] private float _hitRadius = 0.5f;
        
        private Transform _target;
        private float _damage;
        private float _maxSpeed;
        
        private Vector3 _startPoint;
        private Vector3 _fixedTargetPosition;
        private float _t;
        private float _flightDuration;
        
        private float _homingTimer;
        private bool _isHoming;
        
        private AnimationCurve _speedCurve;
        
        private Vector3 _targetVelocityAtShot;
        
        private void Start()
        {
            _startPoint = transform.position;

            switch (_trackingMode)
            {
                case ProjectileTrackingMode.FixedPoint:
                    _fixedTargetPosition = _target != null ? _target.position : transform.position;
                    break;

                case ProjectileTrackingMode.HomingThenStraight:
                    _fixedTargetPosition = _target != null ? _target.position : transform.position;
                    _homingTimer = _homingDuration;
                    _isHoming = true;
                    break;

                case ProjectileTrackingMode.PredictiveShot:
                    _fixedTargetPosition = CalculatePredictedPosition();
                    break;
            }

            RecalculateFlightDuration();
            _projectileVisual.Initialize(_arcHeight);
        }

        private void Update()
        {
            UpdateHoming();
            AdvanceT();
            UpdateRealPosition();
            CheckArrival();
        }
        
        private Vector3 CalculatePredictedPosition()
        {
            if (_target == null) return transform.position;

            Vector3 predictedPosition = _target.position;

            for (int i = 0; i < _predictionIterations; i++)
            {
                float dist = Vector3.Distance(_startPoint, predictedPosition);
                float flightTime = dist / Mathf.Max(_maxSpeed, 0.001f);

                predictedPosition = _target.position + _targetVelocityAtShot * flightTime;
            }

            return predictedPosition;
        }
        
        private void UpdateHoming()
        {
            if (_trackingMode != ProjectileTrackingMode.HomingThenStraight) return;
            if (!_isHoming) return;

            _homingTimer -= Time.deltaTime;

            if (_target != null)
                _fixedTargetPosition = _target.position;

            if (_homingTimer <= 0f)
            {
                _isHoming = false;
                _fixedTargetPosition = _target != null ? _target.position : _fixedTargetPosition;

                _startPoint = transform.position;
                _t = 0f;
                RecalculateFlightDuration();
            }
        }
        
        private void AdvanceT()
        {
            float localSpeed = _speedCurve != null ? _speedCurve.Evaluate(_t) : 1f;
            _t += (localSpeed / _flightDuration) * Time.deltaTime;
            _t = Mathf.Clamp01(_t);
        }

        private void UpdateRealPosition()
        {
            Vector3 realPosition = Vector3.Lerp(_startPoint, _fixedTargetPosition, _t);
            transform.position = realPosition;
            _projectileVisual.UpdateArc(_t, realPosition, _fixedTargetPosition - _startPoint);
        }

        private void CheckArrival()
        {
            if (_t >= 1f ||
                Vector3.Distance(transform.position, _fixedTargetPosition) < _hitRadius)
            {
                if (_target != null &&
                    Vector3.Distance(transform.position, _target.position) < _hitRadius * 2f)
                {
                    _target.GetComponentInParent<IHealth>()?.TakeDamage(_damage);
                }

                Destroy(gameObject);
            }
        }
        
        private void RecalculateFlightDuration()
        {
            float distance = Vector3.Distance(_startPoint, _fixedTargetPosition);
            if (distance < 0.001f) { _flightDuration = 0.01f; return; }

            float avgSpeed = 0f;
            const int samples = 10;
            for (int i = 0; i < samples; i++)
            {
                float s = i / (float)(samples - 1);
                avgSpeed += _speedCurve != null ? _speedCurve.Evaluate(s) : 1f;
            }
            avgSpeed = (avgSpeed / samples) * _maxSpeed;
            _flightDuration = avgSpeed > 0f ? distance / avgSpeed : 1f;
        }
        
        public void InitializeProjectile(Transform target, float maxSpeed, float arcHeight, float damage)
        {
            _target = target;
            _maxSpeed = maxSpeed;
            _arcHeight = arcHeight;
            _damage = damage;
            _fixedTargetPosition = target.position;
            
            var rb = target.GetComponentInParent<Rigidbody2D>();
            if (rb != null)
            {
                _targetVelocityAtShot = rb.linearVelocity;
            }
            else
            {
                _targetVelocityAtShot = Vector3.zero;
            }
        }
        public void InitializeSpeedCurve(AnimationCurve speedCurve)
        {
            _speedCurve = speedCurve;
        }
        
    }
}