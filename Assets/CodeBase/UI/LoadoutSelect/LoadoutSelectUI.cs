using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.States;
using CodeBase.StaticData;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Weapon;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.LoadoutSelect
{
    public class LoadoutSelectUI : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private RoleButton[] roleButtons;
        [SerializeField] private CharacterPanel characterPanel;
        
        private RoleButton _currentFocused;
        private HeroTypeId _selectedHeroType = HeroTypeId.Unknown;
        
        private IGameStateMachine _stateMachine;
        private IPersistentProgressService _progressService;
        private IStaticDataService _staticData;

        private void Awake()
        {
            _stateMachine = AllServices.Container.Single<IGameStateMachine>();
            _progressService = AllServices.Container.Single<IPersistentProgressService>();
            _staticData = AllServices.Container.Single<IStaticDataService>();
            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }
            else
            {
                Debug.LogError("Start Game Button is not assigned in LoadoutSelectUI!");
            }
        }
        private void Start()
        {
            foreach (var btn in roleButtons)
                btn.OnClicked += HandleRoleButtonClicked;
        }
        
        private void HandleRoleButtonClicked(RoleButton clicked)
        {
            _currentFocused?.Unfocus();
            _currentFocused = clicked;
            _currentFocused.Focus();
            HeroStaticData data = clicked.GetData();
            
            _selectedHeroType = data.heroTypeId;
            characterPanel.Show(data);
        }

        private void OnStartGameClicked()
        {
            if (_selectedHeroType == HeroTypeId.Unknown)
            {
                Debug.LogWarning("No hero selected!");
                return;
            }
            
            _progressService.Progress.worldData.selectedHero = _selectedHeroType;

            // Hero chosen — head to the branching map; a node there launches the actual run.
            _stateMachine.Enter<LevelMapState>();
        }
    }
}