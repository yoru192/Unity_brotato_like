using System;
using System.Collections.Generic;
using CodeBase.StaticData;

namespace CodeBase.Data
{
    [Serializable]
    public class PlayerProgress
    {
        public WorldData worldData;
        public State playerState;
        public Dictionary<StatModifierType, float> AppliedUpgrades;
        public PlayerProgress(string initialLevel)
        {
            worldData = new WorldData(initialLevel);
            playerState = new State();
            AppliedUpgrades = new Dictionary<StatModifierType, float>();
        }

    }
}