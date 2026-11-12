namespace CodeBase.Infrastructure.Services.Music
{
    public interface IMusicService : IService
    {
        /// <summary>
        /// Плавно перемикає на трек. Якщо цей трек уже грає — нічого не робить,
        /// тому метод безпечно викликати повторно (наприклад, при виході з паузи).
        /// </summary>
        void Play(MusicTrackId id);

        /// <summary>Плавно вимикає музику.</summary>
        void Stop();
    }

    public enum MusicTrackId
    {
        Map,
        Battle,
        Boss,
        Calm,
        GameOver,
        Win
    }
}