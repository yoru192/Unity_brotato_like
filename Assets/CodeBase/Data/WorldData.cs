using System;
using CodeBase.StaticData.Hero;
using UnityEngine.Serialization;

namespace CodeBase.Data
{
    [Serializable]
    public class WorldData
    {
        public PositionOnLevel positionOnLevel;
        public HeroTypeId selectedHero = HeroTypeId.Unknown;
        
        public WorldData(string initialLevel)
        {
            positionOnLevel = new PositionOnLevel(initialLevel);
        }
    }
}