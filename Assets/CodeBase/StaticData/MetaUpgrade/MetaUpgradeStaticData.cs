using UnityEngine;

namespace CodeBase.StaticData.MetaUpgrade
{
    [CreateAssetMenu(fileName = "MetaUpgrade", menuName = "StaticData/Meta Upgrade")]
    public class MetaUpgradeStaticData : ScriptableObject
    {
        public MetaUpgradeId id;
        public MetaUpgradeBranch branch;
        [Tooltip("Порядок вузла всередині гілки (менше — вище).")]
        public int order;

        [Header("UI Info")]
        public string upgradeName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Effect")]
        [Tooltip("Сила ефекту за один рівень. Що саме означає число — див. MetaUpgradeId.")]
        public float valuePerLevel;

        [Header("Cost")]
        [Tooltip("Ціна кожного рівня в есенції. Кількість елементів = максимальний рівень.")]
        public int[] costs = { 10 };
        [Tooltip("Скільки рівнів інших апгрейдів цієї гілки треба купити, щоб відкрити цей. 0 — доступний одразу.")]
        [Min(0)] public int requiredBranchLevels;

        public int MaxLevel => costs != null ? costs.Length : 0;

        /// <summary>Ціна наступного рівня, якщо зараз куплено <paramref name="currentLevel"/>. -1, якщо рівень уже максимальний.</summary>
        public int CostForNextLevel(int currentLevel) =>
            currentLevel >= 0 && currentLevel < MaxLevel ? costs[currentLevel] : -1;
    }
}
