using UnityEngine;

namespace CodeBase.Enemy
{
    public class CooldownIndicator : MonoBehaviour
    {
        private Animator _animator;
        private IAttackWithCooldown _attack;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();
            
            _attack = GetComponentInParent<EnemyAttack>() as IAttackWithCooldown
                      ?? GetComponentInParent<EnemyRangerAttack>() as IAttackWithCooldown;
        }

        private void OnEnable()
        {
            if (_attack == null) return;
            _attack.OnCooldownStarted += OnCooldownStarted;
            _attack.OnCooldownEnded += OnCooldownEnded;
        }

        private void OnDisable()
        {
            if (_attack == null) return;
            _attack.OnCooldownStarted -= OnCooldownStarted;
            _attack.OnCooldownEnded -= OnCooldownEnded;
        }

        private void OnCooldownStarted() => _animator.SetBool("IsCooldown", false);
        private void OnCooldownEnded() => _animator.SetBool("IsCooldown", true);
    }
}