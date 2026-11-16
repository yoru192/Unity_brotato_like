namespace CodeBase.StaticData.Vfx
{
    /// <summary>
    /// Візуальні ефекти, які програє IVfxService.
    /// УВАГА: значення серіалізуються як числа у VfxStaticData — нові додавай лише в кінець.
    /// </summary>
    public enum VfxId
    {
        None = 0,

        /// <summary>Вибух трупа (перк Death Explosion).</summary>
        DeathExplosion = 1,

        /// <summary>Зациклений вогонь на ворогові, поки він горить (перк Ignite).</summary>
        Burn = 2,

        /// <summary>Спалах льоду в момент сповільнення (перк Frost).</summary>
        FrostHit = 3,

        /// <summary>Удар по ворогу у відповідь (перк Thorns).</summary>
        ThornsHit = 4,

        /// <summary>Порятунок від смерті (перк Second Chance).</summary>
        SecondChance = 5,

        /// <summary>Лікування від удару мечем (перк Vampirism).</summary>
        VampirismHeal = 6,

        /// <summary>Старт ривка з перком Deadly Dash.</summary>
        DashStart = 7,
    }
}
