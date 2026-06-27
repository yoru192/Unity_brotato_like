using BerserkPixel.Tilemap_Generator.SO;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator.Algorithms
{
    public class PerlinNoise : IMapAlgorithm
    {
        private PerlinNoiseMapConfigSO _mapConfig;

        public PerlinNoise(MapConfigSO mapConfig)
        {
            _mapConfig = mapConfig as PerlinNoiseMapConfigSO;
        }
        
        public string GetMapName() => "PN_GeneratedMap_[" + _mapConfig.scale + "][" + _mapConfig.fillPercent + "]";

        public int[,] RandomFillMap()
        {
            var terrainMap = new int[_mapConfig.width, _mapConfig.height];
            var halfWidth = _mapConfig.width / 2f;
            var halfHeight = _mapConfig.height / 2f;
            var innerPercentage = 1f - (_mapConfig.fillPercent / 100f);

            var noiseMap = new float[_mapConfig.width, _mapConfig.height];

            for (int x = 0; x < _mapConfig.width; x++)
            {
                for (int y = 0; y < _mapConfig.height; y++)
                {
                    float xCoord = (x - halfWidth) / _mapConfig.width * _mapConfig.scale + _mapConfig.offsetX;
                    float yCoord = (y - halfHeight) / _mapConfig.height * _mapConfig.scale + _mapConfig.offsetY;

                    float noiseValue = Mathf.PerlinNoise(xCoord, yCoord);
                    noiseMap[x, y] = noiseValue;
                    if (_mapConfig.isIsland)
                    {
                        float xv = x / (float)_mapConfig.width * 2 - 1;
                        float yv = y / (float)_mapConfig.height * 2 - 1;
                        float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));

                        float tripleV = v * v * v;
                        float tripleFallFactor = (_mapConfig.islandSizeFactor - _mapConfig.islandSizeFactor * v) *
                                                 (_mapConfig.islandSizeFactor - _mapConfig.islandSizeFactor * v) *
                                                 (_mapConfig.islandSizeFactor - _mapConfig.islandSizeFactor * v);

                        noiseMap[x, y] -= tripleV / (tripleV + tripleFallFactor);
                    }
                }
            }

            for (int x = 0; x < _mapConfig.width; x++)
            {
                for (int y = 0; y < _mapConfig.height; y++)
                {
                    float noiseValue = noiseMap[x, y];
                    terrainMap[x, y] = GetTile(noiseValue, innerPercentage);
                }
            }

            return terrainMap;
        }

        private int GetTile(float sample, float percentage)
        {
            return percentage < sample ? MapGeneratorConst.TERRAIN_TILE : MapGeneratorConst.DEFAULT_TILE;
        }
    }
}