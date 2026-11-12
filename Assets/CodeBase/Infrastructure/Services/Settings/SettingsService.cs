using System;
using System.Collections.Generic;
using CodeBase.Data;
using UnityEngine;
using UnityEngine.Audio;

namespace CodeBase.Infrastructure.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        private const string PrefsKey = "Settings";

        // AudioMixer має лежати в Assets/Resources/Audio/MainMixer.mixer
        private const string MixerPath = "Audio/MainMixer";
        // Назви exposed-параметрів у міксері
        private const string MasterParam = "MasterVolume";
        private const string MusicParam = "MusicVolume";
        private const string SfxParam = "SfxVolume";

        private SettingsData _data;
        private AudioMixer _mixer;

        public event Action Changed;

        public float MasterVolume
        {
            get => _data.masterVolume;
            set
            {
                _data.masterVolume = Mathf.Clamp01(value);
                ApplyVolume(MasterParam, _data.masterVolume);
                Changed?.Invoke();
            }
        }

        public float MusicVolume
        {
            get => _data.musicVolume;
            set
            {
                _data.musicVolume = Mathf.Clamp01(value);
                ApplyVolume(MusicParam, _data.musicVolume);
                Changed?.Invoke();
            }
        }

        public float SfxVolume
        {
            get => _data.sfxVolume;
            set
            {
                _data.sfxVolume = Mathf.Clamp01(value);
                ApplyVolume(SfxParam, _data.sfxVolume);
                Changed?.Invoke();
            }
        }

        public bool Fullscreen
        {
            get => _data.fullscreen;
            set
            {
                _data.fullscreen = value;
                ApplyDisplay();
                Changed?.Invoke();
            }
        }

        public Vector2Int Resolution
        {
            get => new Vector2Int(_data.resolutionWidth, _data.resolutionHeight);
            set
            {
                _data.resolutionWidth = value.x;
                _data.resolutionHeight = value.y;
                ApplyDisplay();
                Changed?.Invoke();
            }
        }

        public bool VSync
        {
            get => _data.vSync;
            set
            {
                _data.vSync = value;
                ApplyVSync();
                Changed?.Invoke();
            }
        }

        public bool DamageNumbers
        {
            get => _data.damageNumbers;
            set
            {
                _data.damageNumbers = value;
                Changed?.Invoke();
            }
        }

        public bool Stats
        {
            get => _data.stats;
            set
            {
                _data.stats = value;
                Changed?.Invoke();
            }
        }

        public void Load()
        {
            _mixer = Resources.Load<AudioMixer>(MixerPath);
            if (_mixer == null)
                Debug.LogWarning($"AudioMixer не знайдено в Resources/{MixerPath}. Гучність не буде застосована.");

            string json = PlayerPrefs.GetString(PrefsKey, string.Empty);
            _data = string.IsNullOrEmpty(json)
                ? CreateDefault()
                : JsonUtility.FromJson<SettingsData>(json);

            // Захист від битих або порожніх даних
            if (_data == null || _data.resolutionWidth <= 0 || _data.resolutionHeight <= 0)
                _data = CreateDefault();

            ApplyAll();
        }

        public void ApplyAudio()
        {
            ApplyVolume(MasterParam, _data.masterVolume);
            ApplyVolume(MusicParam, _data.musicVolume);
            ApplyVolume(SfxParam, _data.sfxVolume);
        }

        public void Save()
        {
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(_data));
            PlayerPrefs.Save();
        }

        public IReadOnlyList<Vector2Int> GetAvailableResolutions()
        {
            List<Vector2Int> result = new List<Vector2Int>();
            foreach (Resolution r in Screen.resolutions)
            {
                Vector2Int size = new Vector2Int(r.width, r.height);
                if (!result.Contains(size))
                    result.Add(size);
            }

            return result;
        }

        private static SettingsData CreateDefault()
        {
            Resolution desktop = Screen.currentResolution;
            return new SettingsData
            {
                resolutionWidth = desktop.width,
                resolutionHeight = desktop.height,
            };
        }

        private void ApplyAll()
        {
            ApplyVolume(MasterParam, _data.masterVolume);
            ApplyVolume(MusicParam, _data.musicVolume);
            ApplyVolume(SfxParam, _data.sfxVolume);
            ApplyDisplay();
            ApplyVSync();
        }

        private void ApplyVolume(string parameter, float normalized)
        {
            if (_mixer == null)
                return;

            // Лінійний слайдер -> децибели. 0 = повна тиша (-80 dB).
            float db = normalized <= 0.0001f ? -80f : Mathf.Log10(normalized) * 20f;
            _mixer.SetFloat(parameter, db);
        }

        private void ApplyDisplay()
        {
            FullScreenMode mode = _data.fullscreen
                ? FullScreenMode.FullScreenWindow
                : FullScreenMode.Windowed;

            Screen.SetResolution(_data.resolutionWidth, _data.resolutionHeight, mode);
        }

        private void ApplyVSync() =>
            QualitySettings.vSyncCount = _data.vSync ? 1 : 0;
    }
}