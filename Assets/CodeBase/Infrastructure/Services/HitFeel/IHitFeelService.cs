namespace CodeBase.Infrastructure.Services.HitFeel
{
    public interface IHitFeelService : IService
    {
        /// <summary>Тряска камери. amplitude — у світових одиницях, duration — у секундах (unscaled).</summary>
        void Shake(float amplitude, float duration);

        /// <summary>Коротке "завмирання" гри. Ігнорується, якщо гра вже на паузі або hit stop уже триває.</summary>
        void HitStop(float duration);
    }
}
