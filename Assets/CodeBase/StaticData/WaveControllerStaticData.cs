using System;
using System.Collections.Generic;
using CodeBase.StaticData.Enemy;
using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "WaveData", menuName = "StaticData/Wave")]
    public class WaveControllerStaticData : ScriptableObject
    {
        [Header("Wave Settings")]
        public int waveValueMultiplier = 10;
        public int maxEnemiesPerWave = 50;
        public float initialSpawnTimer;
        public int waveDuration = 60;
        public float spawnInterval = 2f;
        public int maxAlive = 10;

        [Header("Enemy Configs")]
        public List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();

        [Serializable]
        public class EnemyConfig
        {
            public EnemyTypeId enemyTypeId;
            public int cost = 1;
        }
    }
}