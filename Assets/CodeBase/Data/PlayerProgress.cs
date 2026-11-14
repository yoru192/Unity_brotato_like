using System;
using System.Collections.Generic;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Data
{
    [Serializable]
    public class PlayerProgress : ISerializationCallbackReceiver
    {
        public WorldData worldData;
        public State playerState;

        // JsonUtility не серіалізує Dictionary, тому у збереження він потрапляє через appliedUpgradeEntries.
        public Dictionary<StatModifierType, float> AppliedUpgrades;
        public MapProgress mapProgress;

        [SerializeField] private List<AppliedUpgradeEntry> appliedUpgradeEntries = new List<AppliedUpgradeEntry>();

        public PlayerProgress(string initialLevel)
        {
            worldData = new WorldData(initialLevel);
            playerState = new State();
            AppliedUpgrades = new Dictionary<StatModifierType, float>();
            mapProgress = new MapProgress();
        }

        public void OnBeforeSerialize()
        {
            appliedUpgradeEntries.Clear();
            if (AppliedUpgrades == null) return;

            foreach (KeyValuePair<StatModifierType, float> pair in AppliedUpgrades)
                appliedUpgradeEntries.Add(new AppliedUpgradeEntry { type = pair.Key, value = pair.Value });
        }

        public void OnAfterDeserialize()
        {
            AppliedUpgrades = new Dictionary<StatModifierType, float>();
            if (appliedUpgradeEntries == null) return;

            foreach (AppliedUpgradeEntry entry in appliedUpgradeEntries)
                AppliedUpgrades[entry.type] = entry.value;
        }

        [Serializable]
        private struct AppliedUpgradeEntry
        {
            public StatModifierType type;
            public float value;
        }
    }
}
