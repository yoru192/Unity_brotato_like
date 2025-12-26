using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Logic
{
    public class SpawnPoint : MonoBehaviour
    {
        public EnemyTypeId enemyTypeId;
        
        private IGameFactory _factory;

        private void Awake()
        {
            _factory = AllServices.Container.Single<IGameFactory>();
        }

        private void Start()
        {
            Spawn();
        }

        private async void Spawn()
        {
            await _factory.CreateEnemy(enemyTypeId, transform);

        }
    }
}