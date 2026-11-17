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
    /// clickable; locked/completed nodes are shown dimmed. The visual hierarchy (ring frame, inner
    /// background, icon, glow, type label) lives in the prefab; this class only tints it per state.
    /// </summary>
    public class MapNodeView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject availableHighlight;
        [SerializeField] private Image background;
        [SerializeField] private Image frame;
        [Tooltip("Node type caption (FIGHT, SHOP, ...); shown only on nodes the player can pick.")]
        [SerializeField] private TextMeshProUGUI typeLabel;

        [Header("Type Icons")]
        [SerializeField] private Sprite combatSprite;
        [SerializeField] private Sprite eliteSprite;
        [SerializeField] private Sprite shopSprite;
        [SerializeField] private Sprite bossSprite;
        [SerializeField] private Sprite campfireSprite;

        [Header("State Colors")]
        [SerializeField] private Color lockedColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color completedColor = new Color(0.65f, 0.6f, 0.55f, 1f);
        [SerializeField] private Color availableFrameColor = new Color(0.98f, 0.78f, 0.3f, 1f);
        [SerializeField] private Color completedFrameColor = new Color(0.6f, 0.52f, 0.36f, 1f);
        [SerializeField] private Color lockedFrameColor = new Color(0.432f, 0.347f, 0.462f, 1f);
        [SerializeField] private Color completedBackgroundColor = new Color(0.18f, 0.16f, 0.22f, 1f);
        [SerializeField] private Color lockedBackgroundColor = new Color(0.13f, 0.11f, 0.16f, 1f);
        [Tooltip("How much the type accent is darkened for the background of an available node.")]
        [Range(0f, 1f)]
        [SerializeField] private float availableBackgroundDarken = 0.45f;
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

            if (icon != null)
            {
                icon.sprite = SpriteFor(node.Type);
                icon.preserveAspect = true;
            }

            if (typeLabel != null)
                typeLabel.text = LabelFor(node.Type);

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
            bool available = state == NodeState.Available;

            if (button != null)
                button.interactable = available;

            if (availableHighlight != null)
                availableHighlight.SetActive(available);

            if (icon != null)
                icon.color = ColorFor(state);

            if (background != null)
                background.color = BackgroundFor(state, AccentFor(_node.Type));

            if (frame != null)
                frame.color = FrameFor(state);

            if (typeLabel != null)
                typeLabel.gameObject.SetActive(available);

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
                case NodeState.Available: return Color.Lerp(accent, Color.black, availableBackgroundDarken);
                case NodeState.Completed: return completedBackgroundColor;
                default: return lockedBackgroundColor;
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
    }
}
