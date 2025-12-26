using CodeBase.Infrastructure.Services;

namespace CodeBase.StaticData
{
    public interface IStaticDataService : IService
    {
        void Load();
        LevelStaticData ForLevel(string sceneKey);
        WeaponStaticData ForWeapon(WeaponTypeId weaponId);
    }
}