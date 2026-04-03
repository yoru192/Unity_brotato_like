using System;
using System.Collections;
using System.Collections.Generic;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Buff
{
    public class BuffService : IBuffService
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IPersistentProgressService _progressService;
        private readonly List<Coroutine> _activeBuffs = new();
        
        public event Action OnWeaponBuffChanged;
        public event Action OnHealthBuffChanged;
        public event Action OnSpeedBuffChanged;
        
        public BuffService(ICoroutineRunner coroutineRunner, IPersistentProgressService progressService)
        {
            _coroutineRunner = coroutineRunner;
            _progressService = progressService;
        }

        public void ApplyBuff(BuffType buffType, float value, float duration)
        {
            Action revert = CreateRevert(buffType, value);
            ApplyStat(buffType, value);

            Coroutine c = _coroutineRunner.StartCoroutine(RevertAfterDelay(duration, revert));
            _activeBuffs.Add(c);
        }

        private void ApplyStat(BuffType buffType, float value)
        {
            State state = _progressService.Progress.playerState;
            switch (buffType)
            {
                case BuffType.Damage:
                    state.MeleeWeaponState.weaponDamage += value;
                    state.RangedWeaponState.weaponDamage += value;
                    OnWeaponBuffChanged?.Invoke();
                    break;
                case BuffType.MaxHealth:
                    state.maxHealth += value;
                    state.currentHealth = Mathf.Min(state.currentHealth + value, state.maxHealth);
                    OnHealthBuffChanged?.Invoke();
                    break;
                case BuffType.MoveSpeed:
                    state.moveSpeed += value;
                    OnSpeedBuffChanged?.Invoke();
                    break;
            }
        }

        private Action CreateRevert(BuffType buffType, float value)
        {
            State state = _progressService.Progress.playerState;
            return buffType switch
            {
                BuffType.Damage => () =>
                {
                    state.MeleeWeaponState.weaponDamage -= value;
                    state.RangedWeaponState.weaponDamage -= value;
                    OnWeaponBuffChanged?.Invoke();
                },
                BuffType.MaxHealth => () =>
                {
                    state.maxHealth -= value;
                    state.currentHealth = Mathf.Min(state.currentHealth, state.maxHealth);
                    OnHealthBuffChanged?.Invoke();
                },
                BuffType.MoveSpeed => () =>
                {
                    state.moveSpeed -= value;
                    OnSpeedBuffChanged?.Invoke();
                },
                _ => null
            };
        }

        private IEnumerator RevertAfterDelay(float duration, Action revert)
        {
            yield return new WaitForSeconds(duration);
            revert?.Invoke();
        }

        public void StopAllBuffs()
        {
            foreach (Coroutine c in _activeBuffs)
                if (c != null) _coroutineRunner.StopCoroutine(c);
            _activeBuffs.Clear();
        }
    }
}