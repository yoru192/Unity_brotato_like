using System.Collections.Generic;
using CodeBase.StaticData.Weapon;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class WeaponHolder : MonoBehaviour
    {
        private readonly List<WeaponBase> _weapons = new();
        private readonly Dictionary<WeaponTypeId, WeaponBase> _weaponMap = new();

        public IReadOnlyList<WeaponBase> Weapons => _weapons;

        public void AddWeapon(WeaponTypeId id, WeaponBase weapon)
        {
            _weapons.Add(weapon);
            _weaponMap[id] = weapon;
        }

        public bool HasWeapon(WeaponTypeId id) => _weaponMap.ContainsKey(id);

        public WeaponBase GetWeapon(WeaponTypeId id) =>
            _weaponMap.TryGetValue(id, out var weapon) ? weapon : null;
    }
}