namespace CodeBase.StaticData.MetaUpgrade
{
    /// <summary>
    /// Гілки дерева мета-апгрейдів. Порядок = індекс контейнера в MetaUpgradeScreen.
    /// Серіалізується як число в ассетах — нові додавай лише в кінець.
    /// </summary>
    public enum MetaUpgradeBranch
    {
        Survival = 0,
        Combat = 1,
        Cunning = 2,
    }
}
