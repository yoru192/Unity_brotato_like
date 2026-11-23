using UnityEngine;

namespace CodeBase.StaticData.Enemy
{
    /// <summary>
    /// Атаки й темп боса. HP, нагороди та префаб беруться з EnemyStaticData з тим самим enemyTypeId.
    /// Ассети лежать у Resources/StaticData/Bosses.
    /// </summary>
    [CreateAssetMenu(fileName = "BossData", menuName = "StaticData/Boss")]
    public class BossStaticData : ScriptableObject
    {
        public EnemyTypeId enemyTypeId;
        [Tooltip("Назва на смузі HP у HUD.")]
        public string displayName = "Boss";

        [Header("Movement")]
        public float moveSpeed = 1.6f;
        [Tooltip("Бос починає атаку, лише коли гравець ближче за цю відстань.")]
        public float engageRange = 14f;

        [Header("Rhythm")]
        [Tooltip("Пауза між атаками (від кінця відновлення до наступного попередження).")]
        public float attackInterval = 2.5f;
        [Tooltip("Скільки бос стоїть і світиться перед атакою — час гравцю зреагувати.")]
        public float telegraphDuration = 0.7f;
        [Tooltip("Скільки бос стоїть після атаки — безпечне вікно для удару.")]
        public float recoverDuration = 0.8f;
        public Color telegraphTint = new Color(0.6f, 1f, 0.3f);

        [Header("Acid Spit")]
        public int spitProjectiles = 3;
        [Tooltip("Кут між крайніми снарядами віяла, градуси.")]
        public float spitSpreadAngle = 40f;
        public float spitDamage = 2f;
        public float spitProjectileSpeed = 9f;

        [Header("Brood")]
        public EnemyTypeId minionTypeId = EnemyTypeId.Larva;
        public int summonCount = 3;
        [Tooltip("Відстань від боса, на якій з'являються міньйони.")]
        public float summonRadius = 2.5f;
        [Tooltip("Більше живих міньйонів бос не кличе — замість виводку плюється.")]
        public int maxAliveMinions = 8;
    }
}
