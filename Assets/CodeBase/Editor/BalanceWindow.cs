using System.Linq;
using CodeBase.Enemy;
using CodeBase.Player.Movement;
using CodeBase.StaticData;
using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Weapon;
using CodeBase.Weapon;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor
{
    public class BalanceWindow : EditorWindow
    {
        private enum Tab
        {
            Player,
            Enemy,
            Weapon,
            Upgrade
        }

        private Tab _tab;

        private PlayerStaticData _player;
        private EnemyStaticData[] _enemies;
        private WeaponStaticData[] _weapons;
        private UpgradeStaticData[] _upgrades;

        private int _selectedEnemy;
        private int _selectedWeapon;
        private int _selectedUpgradeIndex;
        private StatModifierType _selectedUpgradeType;

        [MenuItem("Tools/Balance Window")]
        public static void Open() => GetWindow<BalanceWindow>("Balance");

        private void OnEnable() => Load();

        private void Load()
        {
            _player ??= Resources.Load<PlayerStaticData>("StaticData/Player");

            _enemies = Resources.LoadAll<EnemyStaticData>("StaticData");
            _weapons = Resources.LoadAll<WeaponStaticData>("StaticData");
            _upgrades = Resources.LoadAll<UpgradeStaticData>("StaticData");
        }

        private void OnGUI()
        {
            _tab = (Tab)GUILayout.Toolbar((int)_tab, new[] { "Player", "Enemy", "Weapon",  "Upgrade" });
            GUILayout.Space(10);

            switch (_tab)
            {
                case Tab.Player: DrawPlayer(); break;
                case Tab.Enemy: DrawEnemy(); break;
                case Tab.Weapon: DrawWeapon(); break;
                case Tab.Upgrade: DrawUpgrade(); break;
            }
        }
        
        // ================= PLAYER =================

        private void DrawPlayer()
        {
            if (_player == null)
            {
                EditorGUILayout.HelpBox("PlayerStaticData not found", MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("PLAYER", EditorStyles.boldLabel);
            
            _player.maxStamina = EditorGUILayout.FloatField("Max Stamina", _player.maxStamina);
            _player.regenRateStamina = EditorGUILayout.FloatField("Stamina Regen", _player.regenRateStamina);
            _player.maxHealth = EditorGUILayout.FloatField("Max HP", _player.maxHealth);
            _player.moveSpeed = EditorGUILayout.FloatField("Move Speed", _player.moveSpeed);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_player);
            }
        }
        
        // ================= ENEMY =================

        private void DrawEnemy()
        {
            if (_enemies == null || _enemies.Length == 0)
            {
                EditorGUILayout.HelpBox("EnemyStaticData not found", MessageType.Warning);
                return;
            }

            string[] names = _enemies.Select(e => e.enemyTypeId.ToString()).ToArray();
            _selectedEnemy = EditorGUILayout.Popup("Enemy", _selectedEnemy, names);

            var enemy = _enemies[_selectedEnemy];

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("ENEMY SETTINGS", EditorStyles.boldLabel);

            enemy.health = EditorGUILayout.IntField("Health", enemy.health);
            enemy.damage = EditorGUILayout.IntField("Damage", enemy.damage);
            enemy.moveSpeed = EditorGUILayout.FloatField("Move Speed", enemy.moveSpeed);
            enemy.cooldown = EditorGUILayout.FloatField("Attack Cooldown", enemy.cooldown);
            enemy.radius = EditorGUILayout.FloatField("Attack Radius", enemy.radius);
            enemy.xpReward = EditorGUILayout.IntField("XP Reward", enemy.xpReward);
            enemy.balanceReward = EditorGUILayout.IntField("Gold Reward", enemy.balanceReward);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(enemy);
            }
        }
        
        // ================= WEAPON =================

        private void DrawWeapon()
        {
            if (_weapons == null || _weapons.Length == 0)
            {
                EditorGUILayout.HelpBox("WeaponStaticData not found", MessageType.Warning);
                return;
            }

            string[] names = _weapons.Select(w => w.weaponTypeId.ToString()).ToArray();
            _selectedWeapon = EditorGUILayout.Popup("Weapon", _selectedWeapon, names);

            var weapon = _weapons[_selectedWeapon];

            EditorGUI.BeginChangeCheck();

            GUILayout.Label("WEAPON SETTINGS", EditorStyles.boldLabel);

            weapon.damage = EditorGUILayout.IntField("Damage", weapon.damage);
            weapon.cooldown = EditorGUILayout.FloatField("Cooldown", weapon.cooldown);
            weapon.radius = EditorGUILayout.FloatField("Radius", weapon.radius);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(weapon);
            }
        }
        
        // ================= Upgrades =================

        private void DrawUpgrade()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create New Upgrade", GUILayout.Width(150)))
            {
                CreateNewUpgrade();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            
            if (_upgrades == null || _upgrades.Length == 0)
            {
                EditorGUILayout.HelpBox("UpgradeStaticData not found", MessageType.Warning);
                return;
            }
            
            EditorGUI.BeginChangeCheck();
            _selectedUpgradeType = (StatModifierType)EditorGUILayout.EnumPopup("Upgrade Type", _selectedUpgradeType);
            if (EditorGUI.EndChangeCheck())
            {
                _selectedUpgradeIndex = 0;
            }
            
            var filteredUpgrades = _upgrades.Where(u => u.modifierType == _selectedUpgradeType).ToArray();
            
            if (filteredUpgrades.Length == 0)
            {
                EditorGUILayout.HelpBox($"No upgrades found for type: {_selectedUpgradeType}", MessageType.Info);
                return;
            }
            
            string[] upgradeNames = filteredUpgrades.Select(u => u.upgradeName).ToArray();
            _selectedUpgradeIndex = EditorGUILayout.Popup("Select Upgrade", _selectedUpgradeIndex, upgradeNames);
            _selectedUpgradeIndex = Mathf.Clamp(_selectedUpgradeIndex, 0, filteredUpgrades.Length - 1);
            
            var upgrade = filteredUpgrades[_selectedUpgradeIndex];
            
            EditorGUI.BeginChangeCheck();
            
            GUILayout.Label("UPGRADE SETTINGS", EditorStyles.boldLabel);
            
            upgrade.upgradeName = EditorGUILayout.TextField("Upgrade Name", upgrade.upgradeName);
            upgrade.description = EditorGUILayout.TextField("Description", upgrade.description);
            upgrade.icon = EditorGUILayout.ObjectField("Icon", upgrade.icon, typeof(Sprite), false) as Sprite;
            upgrade.value = EditorGUILayout.FloatField("Value", upgrade.value);
            upgrade.weight = EditorGUILayout.IntField("Weight", upgrade.weight);
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(upgrade);
            }
        }

        private void CreateNewUpgrade()
        {
            UpgradeStaticData newUpgrade = ScriptableObject.CreateInstance<UpgradeStaticData>();
            
            newUpgrade.upgradeName = "New Upgrade";
            newUpgrade.description = "Description";
            newUpgrade.modifierType = _selectedUpgradeType;
            newUpgrade.value = 1f;
            newUpgrade.weight = 1;
            
            string path = "Assets/Resources/StaticData/Upgrades/Upgrade_" + _selectedUpgradeType + "_" + System.DateTime.Now.Ticks + ".asset";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            
            AssetDatabase.CreateAsset(newUpgrade, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newUpgrade;
            
            Load();
            
            Debug.Log($"Created new upgrade at: {path}");
        }
    }
}
