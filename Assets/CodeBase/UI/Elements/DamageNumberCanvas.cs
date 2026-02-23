using UnityEngine;

namespace CodeBase.UI.Elements
{
    /// <summary>
    /// Глобальний Canvas для damage numbers.
    /// Розмістіть на окремому GameObject у сцені — він переживе знищення ворогів.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class DamageNumberCanvas : MonoBehaviour
    {
        public static DamageNumberCanvas Instance { get; private set; }

        public Canvas Canvas { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Canvas = GetComponent<Canvas>();
        }
    }
}