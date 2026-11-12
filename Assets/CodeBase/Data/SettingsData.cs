using System;

namespace CodeBase.Data
{
    /// <summary>
    /// Налаштування гравця. Зберігаються окремо від PlayerProgress (в PlayerPrefs),
    /// тому не скидаються при рестарті забігу.
    /// Гучності нормалізовані 0..1.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        public float masterVolume = 1f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.7f;

        public bool fullscreen = true;
        public int resolutionWidth;
        public int resolutionHeight;
        public bool vSync = true;

        public bool damageNumbers = true;
        public bool stats = true;
    }
}