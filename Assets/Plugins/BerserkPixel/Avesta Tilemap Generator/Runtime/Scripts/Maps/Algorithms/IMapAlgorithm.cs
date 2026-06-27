namespace BerserkPixel.Tilemap_Generator.Algorithms
{
    public interface IMapAlgorithm
    {
        /// <summary>
        /// Generates a terrain in 2D. Each algorithm must generate this somehow.
        /// </summary>
        /// <returns>a 2D array containing either MapGeneratorConst.TERRAIN_TILE or MapGeneratorConst.DEFAULT_TILE values</returns>
        int[,] RandomFillMap();
        
        /// <summary>
        /// To know with what name to Save a Map as a prefab
        /// </summary>
        /// <returns>A string with the name of the prefab</returns>
        string GetMapName();
    }
}