using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.MetaProgress;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.States;
using CodeBase.StaticData;
using CodeBase.StaticData.Hero;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.LoadoutSelect
{
    public class LoadoutSelectUI : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private RoleButton[] roleButtons;
        [SerializeField] private CharacterPanel characterPanel;

        [Header("Meta (опційно)")]
        [Tooltip("Текст із поточною кількістю есенції. Можна не призначати.")]
        [SerializeField] private TextMeshProUGUI essenceText;
        [Tooltip("Напис на кнопці старту: для закритого героя показує ціну відкриття. Можна не призначати.")]
        [SerializeField] private TextMeshProUGUI startButtonLabel;

        private RoleButton _currentFocused;
        private HeroStaticData _selectedHero;
        private string _defaultStartLabel;

        private IGameStateMachine _stateMachine;
        private IPersistentProgressService _progressService;
        private IStaticDataService _staticData;
        private IMetaProgressService _metaProgress;

        private void Awake()
        {
            _stateMachine = AllServices.Container.Single<IGameStateMachine>();
            _progressService = AllServices.Container.Single<IPersistentProgressService>();
            _staticData = AllServices.Container.Single<IStaticDataService>();
            _metaProgress = AllServices.Container.Single<IMetaProgressService>();

            if (startButtonLabel != null)
                _defaultStartLabel = startButtonLabel.text;

            if (startGameButton != null)
            {
                startGameButton.onClick.AddListener(OnStartGameClicked);
            }
            else
            {
                Debug.LogError("Start Game Button is not assigned in LoadoutSelectUI!");
            }
        }

        private void OnEnable()
        {
            if (_metaProgress != null)
                _metaProgress.Changed += RefreshMeta;
        }

        private void OnDisable()
        {
            if (_metaProgress != null)
                _metaProgress.Changed -= RefreshMeta;
        }

        private void Start()
        {
            RoleButton first = null;
            foreach (var btn in roleButtons)
            {
                // Placeholder slots without a hero are hidden; a slot appears once its heroTypeId is set.
                btn.gameObject.SetActive(btn.HasHero);
                if (!btn.HasHero)
                    continue;

                btn.OnClicked += HandleRoleButtonClicked;
                if (first == null)
                    first = btn;
            }

            // Show a real hero right away instead of an empty "?" panel.
            if (first != null)
                HandleRoleButtonClicked(first);
            else
                RefreshMeta();
        }

        private void HandleRoleButtonClicked(RoleButton clicked)
        {
            _currentFocused?.Unfocus();
            _currentFocused = clicked;
            _currentFocused.Focus();

            _selectedHero = clicked.GetData();
            characterPanel.Show(_selectedHero);
            RefreshMeta();
        }

        private void OnStartGameClicked()
        {
            if (_selectedHero == null || _selectedHero.heroTypeId == HeroTypeId.Unknown)
            {
                Debug.LogWarning("No hero selected!");
                return;
            }

            // Закритий герой: перший клік купує його за есенцію, гра стартує наступним кліком.
            if (!_metaProgress.IsHeroUnlocked(_selectedHero))
            {
                if (!_metaProgress.TryUnlockHero(_selectedHero))
                    Debug.LogWarning($"Not enough essence to unlock {_selectedHero.HeroName}.");
                return;
            }

            _progressService.Progress.worldData.selectedHero = _selectedHero.heroTypeId;
            // Герой відомий лише тут, тому бонуси дерева мета-апгрейдів накладаємо саме зараз.
            _metaProgress.ApplyRunBonuses(_selectedHero);

            // Hero chosen — head to the branching map; a node there launches the actual run.
            _stateMachine.Enter<LevelMapState>();
        }

        private void RefreshMeta()
        {
            if (essenceText != null)
                essenceText.text = _metaProgress.Essence.ToString();

            bool locked = _selectedHero != null && !_metaProgress.IsHeroUnlocked(_selectedHero);

            if (startGameButton != null)
                startGameButton.interactable = !locked || _metaProgress.CanUnlockHero(_selectedHero);

            if (startButtonLabel != null)
                startButtonLabel.text = locked ? $"Unlock ({_selectedHero.unlockCost})" : _defaultStartLabel;
        }
    }
}
