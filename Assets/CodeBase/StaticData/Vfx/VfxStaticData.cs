using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.StaticData.Vfx
{
    /// <summary>Який префаб ефекту відповідає якому VfxId. Міняй префаби тут — код чіпати не треба.</summary>
    [CreateAssetMenu(fileName = "VfxStaticData", menuName = "StaticData/Vfx")]
    public class VfxStaticData : ScriptableObject
    {
        public List<VfxEntry> entries = new List<VfxEntry>();
    }

    [Serializable]
    public class VfxEntry
    {
        public VfxId id;
        public GameObject prefab;

        [Tooltip("Масштаб ефекту.")]
        public float scale = 1f;

        [Tooltip("Зсув від точки появи (або від об'єкта, за яким ефект слідує).")]
        public Vector3 offset;

        [Tooltip("Примусово вимкнути ефект через стільки секунд. 0 — чекати, поки ефект завершиться сам. " +
                 "Потрібно для зациклених префабів, які використовуються як разові.")]
        [Min(0f)] public float lifetime;
    }
}
