using CodeBase.Infrastructure.Services;
using CodeBase.StaticData;
using CodeBase.StaticData.Hero;
using CodeBase.StaticData.Weapon;
using UnityEngine;

namespace CodeBase.UI.LoadoutSelect
{
    public class CharacterPanel : MonoBehaviour
    {
        [Header("Hero Info")]
        [SerializeField] private TMPro.TMP_Text heroNameText;
        [SerializeField] private TMPro.TMP_Text heroClassText;
        [SerializeField] private TMPro.TMP_Text descriptionText;
        [SerializeField] private UnityEngine.UI.Image heroImage;
        [SerializeField] private UnityEngine.UI.Image iconImage;

        [Header("Stat Bars")]
        [SerializeField] private StatBar healthBar;
        [SerializeField] private StatBar damageBar;  
        [SerializeField] private StatBar staminaBar;
        [SerializeField] private StatBar staminaRegenBar;
        [SerializeField] private StatBar moveSpeedBar;
        
        [Header("Bar Max References")]
        [SerializeField] private float maxHealthRef       = 200f;
        [SerializeField] private float maxDamageRef       = 100f;
        [SerializeField] private float maxStaminaRef      = 100f;
        [SerializeField] private float maxStaminaRegenRef = 10f;
        [SerializeField] private float maxSpeedRef        = 10f;

        private IStaticDataService _staticData;

        private void Awake()
        {
            _staticData = AllServices.Container.Single<IStaticDataService>();
        }

        public void Show(HeroStaticData heroData)
        {
            // --- Info ---
            if (heroNameText)   heroNameText.text   = heroData.HeroName;
            if (heroClassText)  heroClassText.text  = heroData.HeroClass;
            if (descriptionText) descriptionText.text = heroData.Description;
            if (heroImage)      heroImage.sprite    = heroData.HeroImage;
            if (iconImage)      iconImage.sprite    = heroData.Icon;

            // --- Стати героя ---
            healthBar?.SetValue(heroData.maxHealth / maxHealthRef);
            staminaBar?.SetValue(heroData.maxStamina / maxStaminaRef);
            staminaRegenBar?.SetValue(heroData.regenRateStamina / maxStaminaRegenRef);
            moveSpeedBar?.SetValue(heroData.moveSpeed / maxSpeedRef);

            // --- Дамаг зброї ---
            float totalDamage = GetTotalWeaponDamage(heroData);
            damageBar?.SetValue(totalDamage / maxDamageRef);
        }

        private float GetTotalWeaponDamage(HeroStaticData heroData)
        {
            float total = 0f;
            foreach (WeaponTypeId weaponId in heroData.startWeapons)
            {
                WeaponStaticData weaponData = _staticData.ForWeapon(weaponId);
                if (weaponData != null)
                    total += weaponData.damage;
            }
            return total;
        }
    }
}