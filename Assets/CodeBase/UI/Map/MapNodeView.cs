using System;
using CodeBase.Infrastructure.Map;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.Map
{
    /// <summary>
    /// Visual + button for a single map node. Only <see cref="NodeState.Available"/> nodes are
    /// clickable; locked/completed nodes are shown dimmed.
    /// </summary>
    public class MapNodeView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject availableHighlight;

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

        private MapNode _node;
        private Action<MapNode> _onClicked;

        public MapNode Node => _node;
        public RectTransform RectTransform => (RectTransform)transform;

        public void Construct(MapNode node, Action<MapNode> onClicked)
        {
            _node = node;
            _onClicked = onClicked;

            if (icon != null)
                icon.sprite = SpriteFor(node.Type);

            ApplyState(node.State);

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(RaiseClicked);
            }
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
