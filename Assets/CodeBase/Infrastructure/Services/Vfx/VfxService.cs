using System.Collections.Generic;
using CartoonFX;
using CodeBase.StaticData;
using CodeBase.StaticData.Vfx;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Vfx
{
    public class VfxService : IVfxService
    {
        private readonly Dictionary<VfxId, VfxEntry> _entries = new Dictionary<VfxId, VfxEntry>();
        private readonly Dictionary<VfxId, List<VfxInstance>> _pools = new Dictionary<VfxId, List<VfxInstance>>();

        private Transform _root;

        public VfxService(IStaticDataService staticData)
        {
            VfxStaticData data = staticData.GetVfx();
            if (data == null)
            {
                Debug.LogWarning("VfxStaticData не знайдено в Resources/StaticData/Vfx. Ефекти перків вимкнені.");
                return;
            }

            foreach (VfxEntry entry in data.entries)
            {
                if (entry != null && entry.id != VfxId.None && entry.prefab != null)
                    _entries[entry.id] = entry;
            }

            // Тряску камери робить IHitFeelService; вбудована тряска ефектів Cartoon FX конфліктувала б із CameraFollow.
            CFXR_Effect.GlobalDisableCameraShake = true;
        }

        public void Play(VfxId id, Vector3 position, float scaleMultiplier = 1f)
        {
            if (!_entries.TryGetValue(id, out VfxEntry entry)) return;

            VfxInstance instance = Get(id, entry);
            instance.transform.position = position + entry.offset;
            instance.transform.localScale = Vector3.one * (entry.scale * scaleMultiplier);
            instance.Setup(null, entry.offset, entry.lifetime);
            instance.gameObject.SetActive(true);
        }

        public GameObject PlayFollowing(VfxId id, Transform target)
        {
            if (target == null || !_entries.TryGetValue(id, out VfxEntry entry)) return null;

            VfxInstance instance = Get(id, entry);
            instance.transform.position = target.position + entry.offset;
            instance.transform.localScale = Vector3.one * entry.scale;
            instance.Setup(target, entry.offset, entry.lifetime);
            instance.gameObject.SetActive(true);
            return instance.gameObject;
        }

        public void Stop(GameObject instance)
        {
            if (instance != null)
                instance.SetActive(false);
        }

        /// <summary>Повертає неактивний екземпляр із пулу або створює новий. Екземпляр повертається вимкненим.</summary>
        private VfxInstance Get(VfxId id, VfxEntry entry)
        {
            if (!_pools.TryGetValue(id, out List<VfxInstance> pool))
            {
                pool = new List<VfxInstance>();
                _pools.Add(id, pool);
            }

            for (int i = pool.Count - 1; i >= 0; i--)
            {
                VfxInstance pooled = pool[i];
                if (pooled == null)
                {
                    pool.RemoveAt(i); // знищений ззовні
                    continue;
                }

                if (!pooled.gameObject.activeSelf)
                    return pooled;
            }

            VfxInstance created = Create(entry);
            pool.Add(created);
            return created;
        }

        private VfxInstance Create(VfxEntry entry)
        {
            if (_root == null)
            {
                GameObject rootGo = new GameObject("VfxPool");
                Object.DontDestroyOnLoad(rootGo);
                _root = rootGo.transform;
            }

            // Створюємо вимкненим, щоб ефект не програвся до налаштування позиції.
            bool prefabWasActive = entry.prefab.activeSelf;
            entry.prefab.SetActive(false);
            GameObject go = Object.Instantiate(entry.prefab, _root);
            entry.prefab.SetActive(prefabWasActive);

            // Ефекти Cartoon FX після завершення типово знищують себе — для пулу треба лише вимикати.
            foreach (CFXR_Effect effect in go.GetComponentsInChildren<CFXR_Effect>(true))
            {
                if (effect.clearBehavior == CFXR_Effect.ClearBehavior.Destroy)
                    effect.clearBehavior = CFXR_Effect.ClearBehavior.Disable;
            }

            return go.AddComponent<VfxInstance>();
        }
    }
}
