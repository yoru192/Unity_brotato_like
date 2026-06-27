using System.Globalization;
using BerserkPixel.Tilemap_Generator.SO;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator.Algorithms
{
    public class BasicRandom : IMapAlgorithm
    {
        private BasicRandomSO _mapConfig;

        public BasicRandom(MapConfigSO mapConfig)
        {
            _mapConfig = mapConfig as BasicRandomSO;
        }
        
        public string GetMapName() => $"Random_GeneratedMap_[{_mapConfig.seed}][{_mapConfig.fillPercent}]";
        
        public int[,] RandomFillMap()
        {
            var terrainMap = new int[_mapConfig.width, _mapConfig.height];
            
            if (_mapConfig.useRandomSeed)
            {
                var random = (float)(Time.time / new System.Random().NextDouble());
                _mapConfig.seed = random.ToString(CultureInfo.CurrentCulture);
            }
            
            var pseudoRandom = new System.Random(_mapConfig.seed.GetHashCode());

            for (int x = 0; x < _mapConfig.width; x ++) 
            {
                for (int y = 0; y < _mapConfig.height; y ++) 
                {
                    int value = (pseudoRandom.Next(0,100) < _mapConfig.fillPercent)? 
                        MapGeneratorConst.TERRAIN_TILE : MapGeneratorConst.DEFAULT_TILE;
                    terrainMap[x, y] = value;
                }
            }

            return terrainMap;
        }
    }
}