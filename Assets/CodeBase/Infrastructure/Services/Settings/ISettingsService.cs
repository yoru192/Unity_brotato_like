using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Settings
{
    public interface ISettingsService : IService
    {
        event Action Changed;

        float MasterVolume { get; set; }
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }

        bool Fullscreen { get; set; }
        Vector2Int Resolution { get; set; }
        bool VSync { get; set; }

        bool DamageNumbers { get; set; }
        bool Stats { get; set; }

        /// <summary>Унікальні роздільності (без дублікатів за частотою оновлення).</summary>
        IReadOnlyList<Vector2Int> GetAvailableResolutions();

        void Load();
        void Save();

        /// <summary>
        /// Повторно застосовує гучність до AudioMixer. Потрібно викликати не раніше наступного кадру після
        /// старту гри: AudioMixer.SetFloat, викликаний під час Awake, Unity ігнорує.
        /// </summary>
        void ApplyAudio();
    }
}