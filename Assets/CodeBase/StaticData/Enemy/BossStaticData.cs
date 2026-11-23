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

        [Header("Egg Barrage")]
        public int eggCount = 3;
        [Tooltip("Перше яйце падає точно на гравця, решта — випадково в цьому радіусі навколо нього.")]
        public float eggScatterRadius = 2.5f;
        [Tooltip("Скільки коло на землі попереджає, перш ніж яйце впаде.")]
        public float eggWarningDuration = 1f;
        public float eggImpactRadius = 1.3f;
        public float eggDamage = 3f;
        [Tooltip("Скільки яєць з одного залпу вилуплюються в міньйона.")]
        public int eggsThatHatch = 1;
        public Color eggWarningColor = new Color(1f, 0.25f, 0.2f, 0.45f);

        [Header("Phase 2")]
        [Tooltip("Частка HP, на якій бос переходить у другу фазу.")]
        [Range(0f, 1f)] public float phase2HealthFraction = 0.5f;
        [Tooltip("Ричання: бос стоїть і не отримує урону.")]
        public float phaseShiftDuration = 1.2f;
        [Tooltip("Множник пауз (між атаками, попередження, відновлення) у другій фазі.")]
        public float phase2TempoMultiplier = 0.7f;
        public int phase2SpitProjectiles = 5;
        public int phase2EggCount = 5;
        [Tooltip("Постійний відтінок у другій фазі.")]
        public Color phase2Tint = new Color(0.8f, 1f, 0.65f);

        [Header("Intro / Death")]
        public float introShakeAmplitude = 0.25f;
        public float introShakeDuration = 0.6f;
        public float deathHitStop = 0.15f;
        public float deathShakeAmplitude = 0.35f;
        public float deathShakeDuration = 0.5f;
    }
}
