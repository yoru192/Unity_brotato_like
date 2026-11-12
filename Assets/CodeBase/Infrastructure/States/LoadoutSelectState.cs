using CodeBase.Infrastructure.Services.Music;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class LoadoutSelectState : IState
    {
        private const string LoadoutSelect = "LoadoutSelect";
        private readonly SceneLoader _sceneLoader;
        private readonly IMusicService _music;

        public LoadoutSelectState(SceneLoader sceneLoader, IMusicService musicService)
        {
            _sceneLoader = sceneLoader;
            _music =  musicService;
        }
        
        public void Enter()
        {
            _music.Play(MusicTrackId.Map);
            Debug.Log("Entering LoadoutSelectState");
            _sceneLoader.Load(LoadoutSelect);
        }
        
        public void Exit()
        {
            
        }
    }
}