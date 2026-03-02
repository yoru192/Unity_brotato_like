using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBase.Logic
{
    [RequireComponent(typeof(SortingGroup))]
    public class YSorting : MonoBehaviour
    {
        [Tooltip("Порожній child внизу спрайта (біля ніг). Якщо null — використовується transform цього об'єкта.")]
        [SerializeField] private Transform _sortPoint;

        [Tooltip("Множник для Y-координати. 100 — для невеликих рівнів, 1000 — якщо персонажі стоять близько по Y.")]
        [SerializeField] private int _multiplier = 100;

        [Tooltip("Базовий зсув. Має бути більшим за (maxY * multiplier) + |Order фону|. " +
                 "Гарантує що навіть у верхній частині карти order залишається вищим за фон. " +
                 "За замовчуванням 5000 покриває карти розміром до 50 юнітів по Y при multiplier=100.")]
        [SerializeField] private int _baseOrder = 5000;

        private SortingGroup _sortingGroup;

        private void Awake()
        {
            _sortingGroup = GetComponent<SortingGroup>();

            if (_sortPoint == null)
                _sortPoint = transform;
        }

        private void LateUpdate()
        {
            _sortingGroup.sortingOrder = _baseOrder - (int)(_sortPoint.position.y * _multiplier);
        }
    }
}