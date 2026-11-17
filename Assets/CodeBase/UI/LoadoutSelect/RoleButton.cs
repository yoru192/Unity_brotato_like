using System;
using CodeBase.Infrastructure.Services;
using CodeBase.StaticData;
using CodeBase.StaticData.Hero;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI.LoadoutSelect
{
    public class RoleButton : MonoBehaviour
    {
        [SerializeField] private GameObject focusHighlight;
        [SerializeField] private HeroTypeId heroTypeId;
        private IStaticDataService _staticDataService;
        private HeroStaticData _heroStaticData;
        public bool IsFocused { get; private set; }

        public event Action<RoleButton> OnClicked;

        private void Awake()
        {
            _staticDataService = AllServices.Container.Single<IStaticDataService>();
            // Loaded in Awake so LoadoutSelectUI.Start can already read it (auto-select, hiding empty slots).
            _heroStaticData = _staticDataService.ForHero(heroTypeId);
            GetComponentInChildren<Button>().onClick.AddListener(() => OnClicked?.Invoke(this));
        }

        /// <summary>False for placeholder slots (no hero assigned) — those are hidden.</summary>
        public bool HasHero => heroTypeId != HeroTypeId.Unknown && _heroStaticData != null;
        
        public void Focus()
        {
            IsFocused = true;
            focusHighlight.SetActive(true);
        }

        public void Unfocus()
        {
            IsFocused = false;
            focusHighlight.SetActive(false);
        }

        public HeroStaticData GetData()
        {
            return _heroStaticData;
        }
    }
}