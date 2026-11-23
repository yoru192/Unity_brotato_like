using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Weapon.ArrowWeapon
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

        /// <param name="missSeverity">0..1, наскільки далеко за межею гарантованого влучання стався постріл. Впливає на радіус відхилу при промаху.</param>
        /// <param name="isCritical">Чи був цей постріл критичним. Летить разом зі снарядом і застосовується в момент влучання.</param>
        public void Launch(Transform target, float speed, float damage, bool isMiss = false, float missSeverity = 0f,
            bool isCritical = false)
        {
            var go = ObjectPoolManager.SpawnObject(_projectilePrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Projectile);
            var projectile = go.GetComponent<Projectile>();
            projectile.InitializeProjectile(target, speed, _arcHeight, damage, isMiss, missSeverity, isCritical);
            if (_speedCurve != null && _speedCurve.keys.Length > 0)
                projectile.InitializeSpeedCurve(_speedCurve);
            projectile.Launch();
        }

        /// <summary>Постріл у точку цілі + aimOffset (для віяла). Влучає, лише якщо ціль там опиниться.</summary>
        public void LaunchWithOffset(Transform target, float speed, float damage, Vector3 aimOffset)
        {
            var go = ObjectPoolManager.SpawnObject(_projectilePrefab, transform.position, Quaternion.identity, ObjectPoolManager.PoolType.Projectile);
            var projectile = go.GetComponent<Projectile>();
            projectile.InitializeProjectile(target, speed, _arcHeight, damage);
            projectile.SetAimOffset(aimOffset);
            if (_speedCurve != null && _speedCurve.keys.Length > 0)
                projectile.InitializeSpeedCurve(_speedCurve);
            projectile.Launch();
        }
    }
}