using UnityEngine;

namespace CodeBase.Enemy
{
    public class CooldownIndicator : MonoBehaviour
    {
        private Animator _animator;
        private EnemyAttack _enemyAttack;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            _enemyAttack = GetComponentInParent<EnemyAttack>();
        }

        private void OnEnable()
        {
            if (_enemyAttack == null) return;
            _enemyAttack.OnCooldownStarted += OnCooldownStarted;
            _enemyAttack.OnCooldownEnded += OnCooldownEnded;
        }

        private void OnDisable()
        {
            if (_enemyAttack == null) return;
            _enemyAttack.OnCooldownStarted -= OnCooldownStarted;
            _enemyAttack.OnCooldownEnded -= OnCooldownEnded;
        }

        private void OnCooldownStarted() => _animator.SetBool("IsCooldown", false);
        private void OnCooldownEnded() => _animator.SetBool("IsCooldown", true);
    }
}