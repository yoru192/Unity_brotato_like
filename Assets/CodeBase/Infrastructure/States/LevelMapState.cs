using CodeBase.Infrastructure.Services.Map;
using CodeBase.Infrastructure.Services.Music;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    /// <summary>
    /// Shows the branching level map. Builds (or restores) the map, then loads the map scene whose
    /// UI lets the player pick the next node. Re-entered after every completed run to pick again.
    /// </summary>
    public class LevelMapState : IState
    {
        private const string LevelMapScene = "LevelMap";

        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly IMapService _mapService;
        private readonly IMusicService _music;

        public LevelMapState(GameStateMachine stateMachine, SceneLoader sceneLoader, IMapService mapService, IMusicService musicService)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _mapService = mapService;
            _music =  musicService;
        }

        public void Enter()
        {
            Debug.Log("Entering LevelMapState");
            _mapService.EnsureMap();
            _music.Play(MusicTrackId.Map);

            // A finished map (boss cleared) has no reachable nodes — start a fresh run.
            if (_mapService.IsMapCompleted)
                _mapService.RegenerateMap();

            _sceneLoader.Load(LevelMapScene);
        }

        public void Exit()
        {
        }
    }
}
