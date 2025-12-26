using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.UI.Elements
{
    public class ActorUI : MonoBehaviour
    {
        public HpBar HpBar;
        private IHealth _health;
        
        public void Construct(IHealth health)
        {
            if (_health != null)
            {
                _health.HealthChanged -= UpdateHpBar;
            }
            
            _health = health;
            
            if (_health != null)
            {
                _health.HealthChanged += UpdateHpBar;
                UpdateHpBar();
            }
        }
        
        private void UpdateHpBar()
        {
            if (_health != null && HpBar != null)
            {
                HpBar.SetValue(_health.Current, _health.Max);
            }
        }
        
        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.HealthChanged -= UpdateHpBar;
            }
        }
    }
}