using System.Collections.Generic;
using CodeBase.Infrastructure.Map;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Balance;
using CodeBase.Infrastructure.Services.Map;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Map
{
    /// <summary>
    /// Renders the generated branching map into a scrollable RectTransform: a node view per node,
    /// thin rotated images for the edges, and click handling that launches the chosen run.
    /// Self-wires from the service container like the other scene UIs.
    /// </summary>
    public class MapView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private RectTransform content;
        [SerializeField] private MapNodeView nodePrefab;
        [SerializeField] private RectTransform edgePrefab;
        [SerializeField] private TextMeshProUGUI coinsLabel;

        [Header("Layout")]
        [SerializeField] private Vector2 nodeSize = new Vector2(80f, 80f);
        [SerializeField] private float columnSpacing = 140f;
        [SerializeField] private float rowSpacing = 160f;
        [SerializeField] private float verticalPadding = 120f;
        [SerializeField] private float edgeThickness = 6f;

        [Header("Edge Colors")]
        [SerializeField] private Color edgeColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color openEdgeColor = new Color(1f, 0.9f, 0.4f, 1f);

        private IMapService _mapService;
        private IGameStateMachine _stateMachine;
        private IPersistentProgressService _progress;
        private IBalanceService _balanceService;

        private void Awake()
        {
            _mapService = AllServices.Container.Single<IMapService>();
            _stateMachine = AllServices.Container.Single<IGameStateMachine>();
            _progress = AllServices.Container.Single<IPersistentProgressService>();
            _balanceService = AllServices.Container.Single<IBalanceService>();
        }

        private void Start()
        {
            _mapService.EnsureMap();
            Render(_mapService.CurrentMap);

            // Refresh in place when the map changes (e.g. after a shop node, when the scene is not
            // reloaded because we never left it). Subscribed after the first render to avoid a double.
            _mapService.Changed += OnMapChanged;

            _balanceService.OnBalanceChanged += UpdateCoins;
            UpdateCoins();
        }

        private void OnDestroy()
        {
            if (_mapService != null)
                _mapService.Changed -= OnMapChanged;
            if (_balanceService != null)
                _balanceService.OnBalanceChanged -= UpdateCoins;
        }

        private void OnMapChanged() => Render(_mapService.CurrentMap);

        private void UpdateCoins()
        {
            if (coinsLabel != null)
                coinsLabel.text = _balanceService.CurrentBalance.ToString();
        }

        private void Render(GeneratedMap map)
        {
            if (map == null || content == null || nodePrefab == null)
            {
                Debug.LogError("MapView is missing a map or its prefab/content references.");
                return;
            }

            ClearChildren(content);
            ConfigureContent(map);

            Dictionary<MapNode, MapNodeView> views = new Dictionary<MapNode, MapNodeView>();
            foreach (MapNode node in map.AllNodes)
            {
                MapNodeView view = Instantiate(nodePrefab, content);
                SetupChildRect(view.RectTransform);
                view.RectTransform.sizeDelta = nodeSize;
                view.RectTransform.anchoredPosition = PositionOf(node, map);
                view.Construct(node, OnNodeClicked);
                views[node] = view;
            }

            DrawEdges(map, views);
        }

        private void DrawEdges(GeneratedMap map, Dictionary<MapNode, MapNodeView> views)
        {
            if (edgePrefab == null)
                return;

            foreach (MapNode node in map.AllNodes)
            foreach (MapNode next in node.Outgoing)
            {
                bool open = node.State == NodeState.Completed && next.State == NodeState.Available;
                DrawEdge(views[node].RectTransform.anchoredPosition,
                    views[next].RectTransform.anchoredPosition,
                    open ? openEdgeColor : edgeColor);
            }
        }

        private void DrawEdge(Vector2 from, Vector2 to, Color color)
        {
            RectTransform edge = Instantiate(edgePrefab, content);
            SetupChildRect(edge);

            Vector2 delta = to - from;
            float length = delta.magnitude;

            edge.sizeDelta = new Vector2(length, edgeThickness);
            edge.anchoredPosition = from + delta * 0.5f;
            edge.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);

            if (edge.TryGetComponent(out Image image))
                image.color = color;

            edge.SetAsFirstSibling(); // keep edges behind the node views
        }

        private void OnNodeClicked(MapNode node)
        {
            if (!_mapService.SelectNode(node))
                return;

            // Shop and Campfire nodes open an overlay right here; all other nodes launch a combat run.
            if (node.Type == NodeType.Shop)
            {
                _stateMachine.Enter<ShopState>();
                return;
            }

            if (node.Type == NodeType.Campfire)
            {
                _stateMachine.Enter<CampfireState>();
                return;
            }

            string gameScene = _progress.Progress.worldData.positionOnLevel.Level;
            _stateMachine.Enter<LoadLevelState, string>(gameScene);
        }

        private Vector2 PositionOf(MapNode node, GeneratedMap map) => new Vector2(
            (node.Column - (map.Columns - 1) / 2f) * columnSpacing,
            node.Row * rowSpacing + verticalPadding);

        // Force the content rect into a fixed-size, bottom-center-anchored frame so node/edge
        // anchoredPositions are absolute and predictable regardless of how the scene was wired.
        private void ConfigureContent(GeneratedMap map)
        {
            DisableLayout(content);

            content.anchorMin = new Vector2(0.5f, 0f);
            content.anchorMax = new Vector2(0.5f, 0f);
            content.pivot = new Vector2(0.5f, 0f);
            content.localScale = Vector3.one;
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(
                map.Columns * columnSpacing,
                map.Rows * rowSpacing + verticalPadding * 2f);
        }

        // Pin every node/edge to the content's bottom-center so PositionOf maps straight to pixels.
        private static void SetupChildRect(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
        }

        // A LayoutGroup/ContentSizeFitter on the content would override our manual placement.
        private static void DisableLayout(RectTransform rect)
        {
            if (rect.TryGetComponent(out LayoutGroup group))
                group.enabled = false;
            if (rect.TryGetComponent(out ContentSizeFitter fitter))
                fitter.enabled = false;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
        }
    }
}
