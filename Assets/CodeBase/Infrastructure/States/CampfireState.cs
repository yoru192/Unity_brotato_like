using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.Audio;
using CodeBase.Infrastructure.Services.Map;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Upgrade;
using CodeBase.StaticData;
using CodeBase.UI;
using CodeBase.UI.Screens;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.States
{
    /// <summary>
    /// Campfire node, reached only from the level map. Opens as an overlay in the map scene with two
    /// choices: rest to heal 30% of max HP, or take a random upgrade (rolled and revealed on demand).
    /// </summary>
    public class CampfireState : IState
    {
        private const float HealPercentOfMax = 0.30f;

        private readonly IGameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;
        private readonly IMapService _mapService;
        private readonly IUpgradeService _upgradeService;
        private readonly IPersistentProgressService _persistentProgress;
        private readonly IAudioService _audioService;

        private GameObject _campfireUI;

        public CampfireState(IGameStateMachine stateMachine,
            IGameFactory gameFactory,
            IMapService mapService,
            IUpgradeService upgradeService,
            IPersistentProgressService persistentProgress,
            IAudioService audioService)
        {
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
            _mapService = mapService;
            _upgradeService = upgradeService;
            _persistentProgress = persistentProgress;
            _audioService = audioService;
        }

        public void Enter()
        {
            Time.timeScale = 0f;
            _audioService.PlayLoop(AudioClipId.Campfire);
            _ = ShowCampfire();
        }

        public void Exit()
        {
            Time.timeScale = 1f;
            _audioService.StopLoop(AudioClipId.Campfire);
            if (_campfireUI != null)
                Object.Destroy(_campfireUI);
        }

        private async Task ShowCampfire()
        {
            try
            {
                _campfireUI = await _gameFactory.CreateCampfireScreen();

                CampfireScreen screen = _campfireUI.GetComponentInChildren<CampfireScreen>();
                if (screen != null)
                    screen.Construct(OnChooseRest, TakeRandomUpgrade, CloseToMap);
                else
                    Debug.LogError("CampfireScreen component not found!");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        // Rest: heal 30% of max HP, then leave.
        private void OnChooseRest()
        {
            float healAmount = _persistentProgress.Progress.playerState.maxHealth * HealPercentOfMax;
            _persistentProgress.Progress.playerState.Heal(healAmount);
            CloseToMap();
        }


        private UpgradeStaticData TakeRandomUpgrade()
        {
            List<UpgradeStaticData> options = _upgradeService.GenerateUpgradeOptions(1);
            UpgradeStaticData upgrade = options.Count > 0 ? options[0] : null;

            if (upgrade != null)
                _upgradeService.ApplyUpgrade(upgrade);

            return upgrade;
        }

        private void CloseToMap()
        {
            _mapService.CompleteSelectedNode();
            _stateMachine.Enter<LevelMapState>();
        }
    }
}
