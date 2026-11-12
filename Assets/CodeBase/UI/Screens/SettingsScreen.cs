using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services.Settings;
using CodeBase.UI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Screens
{
    /// <summary>
    /// Панель налаштувань. Лежить неактивним дитиною в PauseScreen (або в меню) і відкривається через Open().
    /// Esc закриває лише цю панель.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_Text masterValueText;
        [SerializeField] private TMP_Text musicValueText;
        [SerializeField] private TMP_Text sfxValueText;

        [Header("Video")]
        [SerializeField] private SettingsSwitch fullscreenSwitch;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private SettingsSwitch vSyncSwitch;

        [Header("Gameplay")]
        [SerializeField] private SettingsSwitch damageNumbersSwitch;
        [SerializeField] private SettingsSwitch statsSwitch;

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        private const int SliderSteps = 10;

        private ISettingsService _settings;
        private Action _onClosed;
        private IReadOnlyList<Vector2Int> _resolutions;
        private bool _subscribed;

        public void Open(ISettingsService settings, Action onClosed)
        {
            _settings = settings;
            _onClosed = onClosed;

            gameObject.SetActive(true);

            ConfigureSlider(masterSlider);
            ConfigureSlider(musicSlider);
            ConfigureSlider(sfxSlider);
            PopulateResolutions();
            RefreshControls();
            Subscribe();
        }

        public void Close()
        {
            Unsubscribe();
            _settings?.Save();
            gameObject.SetActive(false);
            _onClosed?.Invoke();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Close();
        }

        private void OnDestroy() => Unsubscribe();

        private static void ConfigureSlider(Slider slider)
        {
            if (slider == null) return;
            slider.minValue = 0;
            slider.maxValue = SliderSteps;
            slider.wholeNumbers = true;
        }

        private void PopulateResolutions()
        {
            _resolutions = _settings.GetAvailableResolutions();

            List<Vector2Int> list = new List<Vector2Int>(_resolutions);
            // Поточної роздільності може не бути у списку (наприклад, у редакторі) — додаємо.
            if (!list.Contains(_settings.Resolution))
                list.Add(_settings.Resolution);
            _resolutions = list;

            List<string> options = new List<string>();
            foreach (Vector2Int r in _resolutions)
                options.Add($"{r.x} x {r.y}");

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
        }

        private void RefreshControls()
        {
            masterSlider.SetValueWithoutNotify(Mathf.Round(_settings.MasterVolume * SliderSteps));
            musicSlider.SetValueWithoutNotify(Mathf.Round(_settings.MusicVolume * SliderSteps));
            sfxSlider.SetValueWithoutNotify(Mathf.Round(_settings.SfxVolume * SliderSteps));
            UpdateLabel(masterValueText, masterSlider.value);
            UpdateLabel(musicValueText, musicSlider.value);
            UpdateLabel(sfxValueText, sfxSlider.value);

            fullscreenSwitch.SetIsOnWithoutNotify(_settings.Fullscreen);
            vSyncSwitch.SetIsOnWithoutNotify(_settings.VSync);
            damageNumbersSwitch.SetIsOnWithoutNotify(_settings.DamageNumbers);
            statsSwitch.SetIsOnWithoutNotify(_settings.Stats);

            int index = IndexOfResolution(_settings.Resolution);
            resolutionDropdown.SetValueWithoutNotify(index);
            resolutionDropdown.RefreshShownValue();

            // У borderless-fullscreen роздільність = розмір робочого столу, тож вибір має сенс лише у віконному режимі.
            resolutionDropdown.interactable = !_settings.Fullscreen;
        }

        private int IndexOfResolution(Vector2Int resolution)
        {
            for (int i = 0; i < _resolutions.Count; i++)
                if (_resolutions[i] == resolution)
                    return i;
            return 0;
        }

        private void Subscribe()
        {
            if (_subscribed) return;
            _subscribed = true;

            masterSlider.onValueChanged.AddListener(OnMasterChanged);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
            sfxSlider.onValueChanged.AddListener(OnSfxChanged);
            fullscreenSwitch.Changed += OnFullscreenChanged;
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            vSyncSwitch.Changed += OnVSyncChanged;
            damageNumbersSwitch.Changed += OnDamageNumbersChanged;
            statsSwitch.Changed += OnStatsChanged;
            backButton.onClick.AddListener(Close);
        }

        private void Unsubscribe()
        {
            if (!_subscribed) return;
            _subscribed = false;

            masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
            musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
            fullscreenSwitch.Changed -= OnFullscreenChanged;
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            vSyncSwitch.Changed -= OnVSyncChanged;
            damageNumbersSwitch.Changed -= OnDamageNumbersChanged;
            statsSwitch.Changed -= OnStatsChanged;
            backButton.onClick.RemoveListener(Close);
        }

        private void OnMasterChanged(float value)
        {
            _settings.MasterVolume = value / SliderSteps;
            UpdateLabel(masterValueText, value);
        }

        private void OnMusicChanged(float value)
        {
            _settings.MusicVolume = value / SliderSteps;
            UpdateLabel(musicValueText, value);
        }

        private void OnSfxChanged(float value)
        {
            _settings.SfxVolume = value / SliderSteps;
            UpdateLabel(sfxValueText, value);
        }

        private void OnFullscreenChanged(bool value)
        {
            _settings.Fullscreen = value;
            resolutionDropdown.interactable = !value;
        }

        private void OnResolutionChanged(int index) =>
            _settings.Resolution = _resolutions[index];

        private void OnVSyncChanged(bool value) =>
            _settings.VSync = value;

        private void OnDamageNumbersChanged(bool value) =>
            _settings.DamageNumbers = value;
        
        private void OnStatsChanged(bool value) =>
            _settings.Stats = value;


        private static void UpdateLabel(TMP_Text label, float value)
        {
            if (label != null)
                label.text = Mathf.RoundToInt(value).ToString();
        }
    }
}