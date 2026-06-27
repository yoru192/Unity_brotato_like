using System.Globalization;
using BerserkPixel.Tilemap_Generator.SO;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator.Algorithms
{
    public class CellularAutomata : IMapAlgorithm
    {
        private CellularMapConfigSO _mapConfig;

        public CellularAutomata(MapConfigSO mapConfig)
        {
            _mapConfig = mapConfig as CellularMapConfigSO;
        }
        
        public string GetMapName() => "CA_GeneratedMap_[" + _mapConfig.seed + "][" + _mapConfig.fillPercent + "]";

        public int[,] RandomFillMap()
        {
            var terrainMap = new int[_mapConfig.width, _mapConfig.height];

            if (_mapConfig.useRandomSeed)
            {
                var random = (float)(Time.time / new System.Random().NextDouble());
                _mapConfig.seed = random.ToString(CultureInfo.CurrentCulture);
            }

            var pseudoRandom = new System.Random(_mapConfig.seed.GetHashCode());

            for (var x = 0; x < _mapConfig.width; x++)
            {
                for (var y = 0; y < _mapConfig.height; y++)
                {
                    if (x == 0 || x == _mapConfig.width - 1 || y == 0 || y == _mapConfig.height - 1)
                        terrainMap[x, y] = MapGeneratorConst.DEFAULT_TILE;
                    else
                        // 0 means terrain, 1 is a default tile
                        terrainMap[x, y] = pseudoRandom.Next(0, 100) < _mapConfig.fillPercent
                            ? MapGeneratorConst.TERRAIN_TILE
                            : MapGeneratorConst.DEFAULT_TILE;
                }
            }

            for (var i = 0; i < _mapConfig.smoothSteps; i++)
            {
                SmoothMap(ref terrainMap);
            }

            return terrainMap;
        }

        private void SmoothMap(ref int[,] terrainMap)
        {
            for (var x = 0; x < _mapConfig.width; x++)
            {
                for (var y = 0; y < _mapConfig.height; y++)
                {
                    var neighbourWallTiles = GetSurroundingWallCount(terrainMap, x, y);

                    if (neighbourWallTiles > _mapConfig.smoothThreshold)
                        terrainMap[x, y] = MapGeneratorConst.DEFAULT_TILE;
                    else if (neighbourWallTiles < _mapConfig.smoothThreshold)
                        terrainMap[x, y] = MapGeneratorConst.TERRAIN_TILE;
                }
            }
        }

        private int GetSurroundingWallCount(int[,] terrainMap, int gridX, int gridY)
        {
            var wallCount = 0;

            for (var neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
            {
                for (var neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
                {
                    if (neighbourX >= 0 && neighbourX < _mapConfig.width && neighbourY >= 0 &&
                        neighbourY < _mapConfig.height)
                    {
                        if (neighbourX != gridX || neighbourY != gridY) wallCount += terrainMap[neighbourX, neighbourY];
                    }
                    else
                    {
                        wallCount++;
                    }
                }
            }

            return wallCount;
        }
    }
}