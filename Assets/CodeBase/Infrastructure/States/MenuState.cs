using CodeBase.Infrastructure.Services.Music;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    /// <summary>
    /// Головне меню. Вхід через машину станів (а не прямий SceneManager.LoadScene) гарантує,
    /// що попередній стан коректно завершиться: поверне timeScale, прибере свої екрани тощо.
    /// </summary>
    public class MenuState : IState
    {
        private const string MenuScene = "Menu";

        private readonly SceneLoader _sceneLoader;
        private readonly IMusicService _music;

        public MenuState(SceneLoader sceneLoader, IMusicService music)
        {
            _sceneLoader = sceneLoader;
            _music = music;
        }

        public void Enter()
        {
            // Поки сцена меню вантажиться, тримаємо гру на паузі: інакше бій на старій сцені
            // на мить ожив би (і гравець міг би, наприклад, померти вже після Give up).
            Time.timeScale = 0f;
            _music.Play(MusicTrackId.Map);
            _sceneLoader.Load(MenuScene, OnLoaded);
        }

        public void Exit()
        {
            Time.timeScale = 1f;
        }

        private void OnLoaded()
        {
            Time.timeScale = 1f;
        }
    }
}
