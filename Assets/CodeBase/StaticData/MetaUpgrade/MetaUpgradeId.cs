namespace CodeBase.StaticData.MetaUpgrade
{
    /// <summary>
    /// Постійні апгрейди за есенцію (дерево між забігами). Ефект кожного читається за id у місці використання
    /// через IMetaProgressService.GetUpgradeValue — окремих класів на апгрейд немає.
    /// УВАГА: значення серіалізуються як числа в ассетах і в збереженні (PlayerPrefs) — нові додавай лише в кінець.
    /// </summary>
    public enum MetaUpgradeId
    {
        None = 0,

        /// <summary>Більше максимального здоров'я. Значення = відсотки до базового HP героя.</summary>
        Toughness = 1,

        /// <summary>Вогнище лікує більше. Значення = додаткові відсотки від max HP.</summary>
        CampfireWarmth = 2,

        /// <summary>Забіг починається з перком SecondChance. Значення = відсоток HP після порятунку.</summary>
        SecondChance = 3,

        /// <summary>Швидша атака. Значення = на скільки секунд менша перезарядка зброї.</summary>
        QuickHands = 4,

        /// <summary>Швидший рух. Значення = додаток до швидкості героя.</summary>
        LightFeet = 5,

        /// <summary>Сильніша зброя. Значення = додаток до шкоди кожної зброї.</summary>
        SharpBlade = 6,

        /// <summary>Золото на старті забігу. Значення = кількість золота.</summary>
        StartingGold = 7,

        /// <summary>Більше есенції за забіг. Значення = відсотки до нагороди.</summary>
        EssenceGreed = 8,

        /// <summary>Безкоштовні рероли на кожному екрані апгрейду. Значення = кількість безкоштовних реролів.</summary>
        FreeReroll = 9,
    }
}
