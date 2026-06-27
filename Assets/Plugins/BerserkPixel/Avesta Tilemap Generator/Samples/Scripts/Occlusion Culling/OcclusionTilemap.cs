using UnityEngine;
using UnityEngine.Tilemaps;

namespace BerserkPixel.Tilemap_Generator.OcclussionCulling
{
    public class OcclusionTilemap : MonoBehaviour, IOcclusionCulling
    {
        private TilemapRenderer[] _tilemapRenderers;
        private bool _isHidden = true;

        private void Awake()
        {
            _tilemapRenderers = GetComponentsInChildren<TilemapRenderer>();
            foreach (var tilemapRenderer in _tilemapRenderers)
            {
                tilemapRenderer.enabled = false;
            }
           
            _isHidden = true;
        }

        public bool OnShown()
        {
            if (_tilemapRenderers == null || _tilemapRenderers.Length <= 0) return false;

            if (_isHidden)
            {
                _isHidden = false;
                foreach (var tilemapRenderer in _tilemapRenderers)
                {
                    tilemapRenderer.enabled = true;
                }
                return true;
            }
            return false;
        }

        public bool OnDisappear()
        {
            if (_tilemapRenderers == null || _tilemapRenderers.Length <= 0) return false;

            if (!_isHidden)
            {
                _isHidden = true;
                foreach (var tilemapRenderer in _tilemapRenderers)
                {
                    tilemapRenderer.enabled = false;
                }
                return true;
            }
            return false;
        }
    }
}