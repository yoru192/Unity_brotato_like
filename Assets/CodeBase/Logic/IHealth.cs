using System;

namespace CodeBase.Logic
{
    public interface IHealth
    {
        float Current { get; set; }
        float Max { get; set; }
        event Action HealthChanged;
        void TakeDamage(float damage);

        /// <summary>Урон із явно відомою ознакою криту (наприклад, стріла, для якої крит кинули ще під час пострілу).</summary>
        void TakeDamage(float damage, bool isCritical);

        /// <summary>Викликається, коли атака мала влучити в цей об'єкт, але промазала (без зміни HP).</summary>
        void Miss();
    }
}