using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class WeaponHolder : MonoBehaviour, ISavedProgress
    {
        public WeaponTypeId weaponTypeId;
        private IGameFactory _factory;
        
        public void Construct(IGameFactory factory) => 
            _factory = factory;
        public void LoadProgress(PlayerProgress progress)
        {
            Spawn();
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            
        }

        private async void Spawn()
        {
            await _factory.CreateWeapon(weaponTypeId, transform);
        }
    }
}