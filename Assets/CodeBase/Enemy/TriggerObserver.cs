using System;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(Collider2D))]
    public class TriggerObserver : MonoBehaviour
    {
        public event Action<Collider2D> TriggerEnter;
        public event Action<Collider2D> TriggerExit;
        public event Action<Collider2D> TriggerStay;

        private void OnTriggerEnter2D(Collider2D other) =>
            TriggerEnter?.Invoke(other);

        private void OnTriggerExit2D(Collider2D other) =>
            TriggerExit?.Invoke(other);

        private void OnTriggerStay2D(Collider2D other) =>
            TriggerStay?.Invoke(other);
    }
}