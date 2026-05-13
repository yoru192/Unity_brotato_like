using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Weapon.RangeWeapon
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject _projectilePrefab;

        [Header("Arc")]
        [Tooltip("Висота дуги снаряда у world units.")]
        [SerializeField] private float _arcHeight = 2f;

        [Header("Speed Curve (опційно)")]
        [Tooltip("Крива швидкості снаряда. X=прогрес польоту, Y=множник швидкості. Залиш пустою для постійної швидкості.")]
        [SerializeField] private AnimationCurve _speedCurve;

        public void Launch(Transform target, float speed, float damage)
        {
            var go = ObjectPoolManager.SpawnObject(_projectilePrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Projectile);
            var projectile = go.GetComponent<Projectile>();
            projectile.InitializeProjectile(target, speed, _arcHeight, damage);
            if (_speedCurve != null && _speedCurve.keys.Length > 0)
                projectile.InitializeSpeedCurve(_speedCurve);
            projectile.Launch();
        }
    }
}
