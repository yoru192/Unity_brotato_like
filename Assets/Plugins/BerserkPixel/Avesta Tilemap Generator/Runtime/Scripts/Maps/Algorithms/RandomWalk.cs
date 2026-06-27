using System.Globalization;
using BerserkPixel.Tilemap_Generator.SO;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator.Algorithms
{
    public class RandomWalk : IMapAlgorithm
    {
        private RandomWalkSO _mapConfig;
        
        public RandomWalk(MapConfigSO mapConfig)
        {
            _mapConfig = mapConfig as RandomWalkSO;
        }
        
        public string GetMapName() => $"RW_GeneratedMap_[{_mapConfig.seed}][{_mapConfig.fillPercent}]";
        
        private int[,] FullDefaultMap()
        {
            var terrainMap = new int[_mapConfig.width, _mapConfig.height];

            for (int x = 0; x < _mapConfig.width; x++)
            {
                for (int y = 0; y < _mapConfig.height; y++)
                {
                    terrainMap[x, y] = _mapConfig.invertGrid? MapGeneratorConst.DEFAULT_TILE : MapGeneratorConst.TERRAIN_TILE;
                }
            }

            return terrainMap;
        }
        
        public int[,] RandomFillMap()
        {
            var terrainMap = FullDefaultMap();
            
            if (_mapConfig.useRandomSeed)
            {
                var random = (float)(Time.time / new System.Random().NextDouble());
                _mapConfig.seed = random.ToString(CultureInfo.CurrentCulture);
            }
            
            var pseudoRandom = new System.Random(_mapConfig.seed.GetHashCode());

            var requiredFillQuantity = (int)(_mapConfig.width * _mapConfig.height * _mapConfig.fillPercent / 100);
            var fillCounter = 0;

            var currentX = _mapConfig.startingPoint.x;
            var currentY = _mapConfig.startingPoint.y;
            terrainMap[currentX, currentY] = 0;
            fillCounter++;
            var iterationsCounter = 0;
            
            while (fillCounter < requiredFillQuantity && iterationsCounter < _mapConfig.maxIterations)
            { 
                int direction = pseudoRandom.Next(4); 

                switch (direction)
                {
                    case 0: 
                        if ((currentY + 1) < _mapConfig.height) 
                        {
                            currentY++;
                            terrainMap = Carve(terrainMap, currentX, currentY, ref fillCounter);
                        }
                        break;
                    case 1: 
                        if ((currentY - 1) > 1)
                        { 
                            currentY--;
                            terrainMap = Carve(terrainMap, currentX, currentY, ref fillCounter);
                        }
                        break;
                    case 2: 
                        if ((currentX - 1) > 1)
                        {
                            currentX--;
                            terrainMap = Carve(terrainMap, currentX, currentY, ref fillCounter);
                        }
                        break;
                    case 3: 
                        if ((currentX + 1) < _mapConfig.width)
                        {
                            currentX++;
                            terrainMap = Carve(terrainMap, currentX, currentY, ref fillCounter);
                        }
                        break;
                }

                iterationsCounter++;
            }

            return terrainMap;
        }
        
        private int[,] Carve(int[,] terrainMap, int x, int y, ref int fillCounter)
        {
            var tile = _mapConfig.invertGrid ? MapGeneratorConst.TERRAIN_TILE : MapGeneratorConst.DEFAULT_TILE;
            var checkTile = _mapConfig.invertGrid ? MapGeneratorConst.DEFAULT_TILE : MapGeneratorConst.TERRAIN_TILE;
            
            if (terrainMap[x, y] != checkTile) return terrainMap;
            
            terrainMap[x, y] = tile;
            fillCounter++;
            return terrainMap;
        }
    }
}