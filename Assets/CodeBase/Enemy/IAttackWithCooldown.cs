using System;

namespace CodeBase.Enemy
{
    public interface IAttackWithCooldown
    {
        event Action OnCooldownStarted;
        event Action OnCooldownEnded;
    }
}