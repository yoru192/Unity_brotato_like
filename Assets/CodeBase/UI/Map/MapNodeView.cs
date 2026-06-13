using System;
using CodeBase.Infrastructure.Map;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CodeBase.UI.Map
{
    /// <summary>
    /// Visual + button for a single map node. Only <see cref="NodeState.Available"/> nodes are
    /// clickable; locked/completed nodes are shown dimmed.
    /// </summary>
    public class MapNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject availableHighlight;
        [SerializeField] private Image background;
        [SerializeField] private Image frame;
        [SerializeField] private TextMeshProUGUI typeLabel;
        [SerializeField] private TextMeshProUGUI floorLabel;
        [SerializeField] private TextMeshProUGUI statusLabel;

        [Header("Type Icons")]
        [SerializeField] private Sprite combatSprite;
        [SerializeField] private Sprite eliteSprite;
        [SerializeField] private Sprite shopSprite;
        [SerializeField] private Sprite bossSprite;
        [SerializeField] private Sprite campfireSprite;

        [Header("State Colors")]
        [SerializeField] private Color lockedColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color completedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        [SerializeField] private Color availableFrameColor = new Color(1f, 0.78f, 0.25f, 1f);
        [SerializeField] private Color completedFrameColor = new Color(0.42f, 0.95f, 0.65f, 1f);
        [SerializeField] private Color lockedFrameColor = new Color(0.36f, 0.39f, 0.48f, 0.85f);
        [SerializeField] private Color combatColor = new Color(0.36f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color eliteColor = new Color(0.95f, 0.38f, 0.32f, 1f);
        [SerializeField] private Color shopColor = new Color(0.32f, 0.78f, 0.58f, 1f);
        [SerializeField] private Color bossColor = new Color(0.9f, 0.28f, 0.76f, 1f);
        [SerializeField] private Color campfireColor = new Color(1f, 0.62f, 0.28f, 1f);

        private MapNode _node;
        private Action<MapNode> _onClicked;
        private Vector3 _baseScale = Vector3.one;
        private bool _hovered;

        public MapNode Node => _node;
        public RectTransform RectTransform => (RectTransform)transform;

        public void Construct(MapNode node, Action<MapNode> onClicked)
        {
            _node = node;
            _onClicked = onClicked;
            _baseScale = transform.localScale;

            EnsureVisuals();

            if (icon != null)
            {
                icon.sprite = SpriteFor(node.Type);
                icon.preserveAspect = true;
            }

            if (typeLabel != null)
                typeLabel.text = LabelFor(node.Type);

            if (floorLabel != null)
                floorLabel.text = $"LVL {node.Row + 1}";

            ApplyState(node.State);

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(RaiseClicked);
            }
        }

        private void Update()
        {
            if (_node == null || _node.State != NodeState.Available)
                return;

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.035f;
            float hover = _hovered ? 1.08f : 1f;
            transform.localScale = _baseScale * pulse * hover;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            if (_node != null && _node.State == NodeState.Available)
                transform.localScale = _baseScale * 1.08f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            transform.localScale = _baseScale;
        }

        private void RaiseClicked() => _onClicked?.Invoke(_node);

        private void ApplyState(NodeState state)
        {
            if (button != null)
                button.interactable = state == NodeState.Available;

            if (availableHighlight != null)
                availableHighlight.SetActive(state == NodeState.Available);

            if (icon != null)
                icon.color = ColorFor(state);

            Color accent = AccentFor(_node.Type);

            if (background != null)
                background.color = BackgroundFor(state, accent);

            if (frame != null)
                frame.color = FrameFor(state);

            if (typeLabel != null)
                typeLabel.color = state == NodeState.Locked ? lockedColor : Color.white;

            if (floorLabel != null)
                floorLabel.color = new Color(1f, 1f, 1f, state == NodeState.Locked ? 0.45f : 0.82f);

            if (statusLabel != null)
            {
                statusLabel.text = StatusFor(state);
                statusLabel.color = FrameFor(state);
            }

            transform.localScale = _baseScale;
        }

        private Color ColorFor(NodeState state)
        {
            switch (state)
            {
                case NodeState.Available: return availableColor;
                case NodeState.Completed: return completedColor;
                default: return lockedColor;
            }
        }

        private Color FrameFor(NodeState state)
        {
            switch (state)
            {
                case NodeState.Available: return availableFrameColor;
                case NodeState.Completed: return completedFrameColor;
                default: return lockedFrameColor;
            }
        }

        private Color BackgroundFor(NodeState state, Color accent)
        {
            switch (state)
            {
                case NodeState.Available: return Color.Lerp(accent, Color.black, 0.28f);
                case NodeState.Completed: return new Color(0.13f, 0.2f, 0.16f, 0.95f);
                default: return new Color(0.08f, 0.09f, 0.12f, 0.82f);
            }
        }

        private Color AccentFor(NodeType type)
        {
            switch (type)
            {
                case NodeType.Elite: return eliteColor;
                case NodeType.Shop: return shopColor;
                case NodeType.Boss: return bossColor;
                case NodeType.Campfire: return campfireColor;
                default: return combatColor;
            }
        }

        private string LabelFor(NodeType type)
        {
            switch (type)
            {
                case NodeType.Elite: return "ELITE";
                case NodeType.Shop: return "SHOP";
                case NodeType.Boss: return "BOSS";
                case NodeType.Campfire: return "REST";
                default: return "FIGHT";
            }
        }

        private string StatusFor(NodeState state)
        {
            switch (state)
            {
                case NodeState.Available: return "READY";
                case NodeState.Completed: return "DONE";
                default: return "LOCKED";
            }
        }

        private Sprite SpriteFor(NodeType type)
        {
            switch (type)
            {
                case NodeType.Elite: return eliteSprite;
                case NodeType.Shop: return shopSprite;
                case NodeType.Boss: return bossSprite;
                case NodeType.Campfire: return campfireSprite;
                default: return combatSprite;
            }
        }

        private void EnsureVisuals()
        {
            Image rootImage = GetComponent<Image>();
            if (background == null)
                background = rootImage != null ? rootImage : gameObject.AddComponent<Image>();

            background.type = Image.Type.Simple;

            if (button != null && button.targetGraphic == null)
                button.targetGraphic = background;

            frame ??= CreateImage("Frame", transform, new Vector2(1f, 1f), new Vector2(10f, 10f));
            frame.type = Image.Type.Simple;
            frame.raycastTarget = false;

            if (icon != null)
            {
                icon.raycastTarget = false;
                icon.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                icon.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                icon.rectTransform.anchoredPosition = new Vector2(0f, 6f);
                icon.rectTransform.sizeDelta = new Vector2(42f, 42f);
            }

            if (availableHighlight != null)
            {
                Image highlight = availableHighlight.GetComponent<Image>() ??
                                  availableHighlight.AddComponent<Image>();
                highlight.type = Image.Type.Simple;
                highlight.color = new Color(1f, 0.76f, 0.2f, 0.22f);
                highlight.raycastTarget = false;
                availableHighlight.transform.SetAsFirstSibling();
            }

            typeLabel ??= CreateLabel("TypeLabel", new Vector2(0.5f, 1f), new Vector2(0f, -7f), 18f, FontStyles.Bold);
            floorLabel ??= CreateLabel("FloorLabel", new Vector2(0.5f, 0f), new Vector2(0f, 8f), 13f, FontStyles.Bold);
            statusLabel ??= CreateLabel("StatusLabel", new Vector2(0.5f, 0f), new Vector2(0f, -18f), 10f, FontStyles.Bold);

            frame.transform.SetAsLastSibling();
            if (icon != null)
                icon.transform.SetAsLastSibling();
            if (typeLabel != null)
                typeLabel.transform.SetAsLastSibling();
            if (floorLabel != null)
                floorLabel.transform.SetAsLastSibling();
            if (statusLabel != null)
                statusLabel.transform.SetAsLastSibling();
        }

        private Image CreateImage(string name, Transform parent, Vector2 anchor, Vector2 padding)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = gameObject.layer;
            go.transform.SetParent(parent, false);

            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = anchor;
            rect.offsetMin = padding;
            rect.offsetMax = -padding;

            return go.GetComponent<Image>();
        }

        private TextMeshProUGUI CreateLabel(string name, Vector2 anchor, Vector2 position, float size, FontStyles style)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);

            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(96f, 22f);

            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = size;
            label.fontStyle = style;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            return label;
        }
    }
}
