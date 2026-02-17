using System.Threading.Tasks;
using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using CodeBase.StaticData.Weapon;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class WeaponHolder : MonoBehaviour, ISavedProgress
    {
        public WeaponTypeId weaponTypeId;
        private IGameFactory _factory;
        private bool _isWeaponSpawned = false;

        public void Construct(IGameFactory factory)
        {
            _factory = factory;
        }

        public async void LoadProgress(PlayerProgress progress)
        {
            if (!_isWeaponSpawned)
            {
                await Spawn();
                _isWeaponSpawned = true;
            }
        }
        public void UpdateProgress(PlayerProgress progress)
        {
        }

        private async Task Spawn()
        {
            await _factory.CreateWeapon(weaponTypeId, transform);
        }
    }

}