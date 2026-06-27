using System;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator
{
    [Serializable]
    public struct TileObjects
    {
        [HideInInspector]
        [SerializeField] private string name;
        
        public GameObject prefab;
        [Range(0, 1)] public float weight;
        
        public void OnBeforeSerialize()
        {
            if (prefab == null) return;

            name = prefab.name;
        }

        public void OnAfterDeserialize()
        {
            if (prefab == null) return;
            
            name = prefab.name;
        }
    }
}