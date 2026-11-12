using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.UI.Elements
{
    /// <summary>
    /// Глобальний Canvas для damage numbers.
    /// Розмістіть на окремому GameObject у сцені — він переживе знищення ворогів.
    /// Тримає пул цифр урону, щоб не робити Instantiate/Destroy на кожен удар.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class DamageNumberCanvas : MonoBehaviour
    {
        public static DamageNumberCanvas Instance { get; private set; }

        public Canvas Canvas { get; private set; }

        private readonly Dictionary<GameObject, Stack<DamageNumber>> _pools = new Dictionary<GameObject, Stack<DamageNumber>>();

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

        /// <summary>Бере цифру урону з пулу (або створює нову) і ставить у задану позицію.</summary>
        public DamageNumber Spawn(GameObject prefab, Vector3 position)
        {
            if (!_pools.TryGetValue(prefab, out Stack<DamageNumber> pool))
            {
                pool = new Stack<DamageNumber>();
                _pools.Add(prefab, pool);
            }

            while (pool.Count > 0)
            {
                DamageNumber pooled = pool.Pop();
                if (pooled == null) continue; // знищений ззовні

                pooled.transform.SetPositionAndRotation(position, Quaternion.identity);
                pooled.gameObject.SetActive(true);
                return pooled;
            }

            GameObject go = Instantiate(prefab, position, Quaternion.identity, Canvas.transform);
            if (!go.TryGetComponent(out DamageNumber number))
            {
                Debug.LogError($"Prefab {prefab.name} не має компонента DamageNumber", prefab);
                Destroy(go);
                return null;
            }

            number.SetPool(this, prefab);
            return number;
        }

        /// <summary>Повертає цифру урону в пул. Викликається самим DamageNumber, коли сплив час життя.</summary>
        public void Release(DamageNumber number, GameObject prefab)
        {
            number.gameObject.SetActive(false);

            if (_pools.TryGetValue(prefab, out Stack<DamageNumber> pool))
                pool.Push(number);
            else
                Destroy(number.gameObject);
        }
    }
}
