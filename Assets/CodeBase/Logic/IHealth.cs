using System;

namespace CodeBase.Logic
{
    public interface IHealth
    {
        float Current { get; set; }
        float Max { get; set; }
        event Action HealthChanged;
        void TakeDamage(float damage);

        /// <summary>Викликається, коли атака мала влучити в цей об'єкт, але промазала (без зміни HP).</summary>
        void Miss();
    }
}