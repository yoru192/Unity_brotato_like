using UnityEngine;
using Febucci.UI;

namespace CodeBase.UI.Elements
{
    public class DamageNumber : MonoBehaviour
    {
        [Header("Час життя")]
        [SerializeField] private float lifetime = 1.2f;

        [Header("Рух")]
        [SerializeField] private float moveSpeedY = 1.5f;
        [SerializeField] private float moveSpeedX = 0.4f;

        [Header("Кольори по черзі (критикал завжди червоний)")]
        [SerializeField] private Color[] sequentialColors = new Color[]
        {
            Color.white,
            Color.yellow,
            Color.cyan,
            Color.green,
            new Color(1f, 0.5f, 0f),   // помаранчевий
            new Color(0.6f, 0.4f, 1f),  // фіолетовий
        };

        // статичний лічильник — спільний для всіх інстансів
        private static int _nextColorIndex = 0;

        private float _timer;
        private float _directionX;
        private TypewriterByCharacter _typewriter;

        private void Awake()
        {
            _typewriter = GetComponent<TypewriterByCharacter>();
        }

        public void Initialize(float damage, bool isCritical = false)
        {
            string text;

            if (isCritical)
            {
                text = $"<color=red><b>-{damage}!</b></color>";
            }
            else
            {
                Color color = GetNextSequentialColor();
                string hex = ColorUtility.ToHtmlStringRGB(color);
                text = $"<color=#{hex}>-{damage}</color>";
            }

            _typewriter.ShowText(text);
            _timer = lifetime;

            // Рандомний напрямок та сила бокового зміщення
            _directionX = Random.Range(-1f, 1f);
        }

        private void Update()
        {
            transform.position += new Vector3(
                _directionX * moveSpeedX * Time.deltaTime,
                moveSpeedY * Time.deltaTime,
                0f
            );

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
                Destroy(gameObject);
        }

        private Color GetNextSequentialColor()
        {
            if (sequentialColors == null || sequentialColors.Length == 0)
                return Color.white;

            Color color = sequentialColors[_nextColorIndex % sequentialColors.Length];
            _nextColorIndex++;
            return color;
        }
    }
}