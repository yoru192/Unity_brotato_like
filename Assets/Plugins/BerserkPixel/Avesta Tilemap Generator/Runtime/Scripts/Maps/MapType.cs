using System;

namespace BerserkPixel.Tilemap_Generator
{
    [Serializable]
    public enum MapType
    {
        CellularAutomata = 0,
        PerlinNoise = 1,
        BasicRandom = 4,
        RandomWalk = 5,
    }
}