using BerserkPixel.Tilemap_Generator.Algorithms;
using BerserkPixel.Tilemap_Generator.SO;

namespace BerserkPixel.Tilemap_Generator
{
    public static class MapTypeExt
    {
        public static MapType GetFromSO(this MapConfigSO mapConfig)
        {
            var selectedType = mapConfig.GetType();

            if (selectedType == typeof(CellularMapConfigSO))
            {
                return MapType.CellularAutomata;
            }

            if (selectedType == typeof(PerlinNoiseMapConfigSO))
            {
                return MapType.PerlinNoise;
            }

            if (selectedType == typeof(BasicRandomSO))
            {
                return MapType.BasicRandom;
            }

            if (selectedType == typeof(RandomWalkSO))
            {
                return MapType.RandomWalk;
            }

            return MapType.CellularAutomata;
        }

        public static IMapAlgorithm GetFromType(this MapType mapType, MapConfigSO mapConfig)
        {
            switch (mapType)
            {
                case MapType.CellularAutomata:
                    return new CellularAutomata(mapConfig);
                case MapType.PerlinNoise:
                    return new PerlinNoise(mapConfig);
                case MapType.BasicRandom:
                    return new BasicRandom(mapConfig);
                case MapType.RandomWalk:
                    return new RandomWalk(mapConfig);
                default:                  
                    return new CellularAutomata(mapConfig);
            }
        }
    }
}