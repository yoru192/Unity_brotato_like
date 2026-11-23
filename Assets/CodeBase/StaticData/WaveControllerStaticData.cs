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
        public int maxWaves = 20;
        public int waveValueMultiplier = 10;
        public int maxEnemiesPerWave = 50;
        public float initialSpawnTimer;
        public int waveDuration = 60;
        public float spawnInterval = 2f;
        public int maxAlive = 10;

        [Header("Boss")]
        [Tooltip("Бій з босом: бос з'являється на старті, а забіг закінчується його смертю (maxWaves ігнорується). " +
                 "Звичайні хвилі з enemyConfigs ідуть далі як фон.")]
        public bool spawnBoss;
        public EnemyTypeId bossTypeId = EnemyTypeId.BroodQueen;
        [Tooltip("Секунди від старту рівня до появи боса.")]
        public float bossSpawnDelay = 2f;
        [Tooltip("Бос з'являється на точці спавну, чия відстань до гравця найближча до цієї (одразу в полі зору).")]
        public float bossSpawnDistance = 9f;
        [Tooltip("Секунди від смерті боса до завершення забігу.")]
        public float bossDefeatedDelay = 3f;

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