using System.Collections.Generic;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.MetaProgress;
using CodeBase.StaticData;
using CodeBase.StaticData.MetaUpgrade;
using CodeBase.UI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Screens
{
    /// <summary>
    /// Дерево постійних апгрейдів за есенцію. Вузли створюються з ассетів Resources/StaticData/MetaUpgrades
    /// і розкладаються по контейнерах гілок. Компонент має висіти на завжди активному об'єкті (напр. Canvas),
    /// а сама панель, яку відкривають і закривають кнопки, задається в <see cref="root"/>.
    /// </summary>
    public class MetaUpgradeScreen : MonoBehaviour
    {
        [SerializeField] private MetaUpgradeNodeView nodePrefab;
        [Tooltip("Контейнери гілок у порядку MetaUpgradeBranch: Survival, Combat, Cunning.")]
        [SerializeField] private Transform[] branchContainers;
        [SerializeField] private TextMeshProUGUI essenceText;

        [Header("Open / close (опційно)")]
        [Tooltip("Панель дерева. Якщо задана — на старті схована і відкривається кнопкою openButton.")]
        [SerializeField] private GameObject root;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        private readonly List<MetaUpgradeNodeView> _nodes = new List<MetaUpgradeNodeView>();

        private IMetaProgressService _metaProgress;
        private IStaticDataService _staticData;

        private void Awake()
        {
            _metaProgress = AllServices.Container.Single<IMetaProgressService>();
            _staticData = AllServices.Container.Single<IStaticDataService>();

            if (openButton != null)
                openButton.onClick.AddListener(Open);
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            BuildNodes();

            if (root != null)
                root.SetActive(false);
        }

        private void OnDestroy()
        {
            if (openButton != null)
                openButton.onClick.RemoveListener(Open);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);
        }

        private void OnEnable()
        {
            if (_metaProgress == null) return;

            _metaProgress.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_metaProgress != null)
                _metaProgress.Changed -= Refresh;
        }

        private void Open()
        {
            if (root != null)
                root.SetActive(true);
            Refresh();
        }

        private void Close()
        {
            if (root != null)
                root.SetActive(false);
        }

        private void BuildNodes()
        {
            if (nodePrefab == null)
            {
                Debug.LogError("MetaUpgradeScreen: nodePrefab is not assigned!");
                return;
            }

            foreach (MetaUpgradeStaticData upgrade in _staticData.GetAllMetaUpgrades())
            {
                int branchIndex = (int)upgrade.branch;
                if (branchContainers == null || branchIndex < 0 || branchIndex >= branchContainers.Length
                    || branchContainers[branchIndex] == null)
                {
                    Debug.LogError($"MetaUpgradeScreen: no container for branch {upgrade.branch} ({upgrade.name}).");
                    continue;
                }

                MetaUpgradeNodeView node = Instantiate(nodePrefab, branchContainers[branchIndex]);
                node.Initialize(upgrade, OnBuyRequested);
                _nodes.Add(node);
            }
        }

        private void OnBuyRequested(MetaUpgradeStaticData upgrade)
        {
            // Успішна купівля викликає Changed, а він — Refresh.
            _metaProgress.TryBuyUpgrade(upgrade);
        }

        private void Refresh()
        {
            if (essenceText != null)
                essenceText.text = _metaProgress.Essence.ToString();

            foreach (MetaUpgradeNodeView node in _nodes)
                node.Refresh(_metaProgress);
        }
    }
}
