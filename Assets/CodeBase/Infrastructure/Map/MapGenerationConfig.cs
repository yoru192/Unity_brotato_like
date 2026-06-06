namespace CodeBase.Infrastructure.Map
{
    /// <summary>
    /// Tunable parameters for procedural map generation. Plain class (no Unity dependency)
    /// so the generator stays unit-testable; a ScriptableObject can feed these values later.
    /// </summary>
    public class MapGenerationConfig
    {
        public int Rows = 12;
        public int Columns = 7;

        /// <summary>How many start-to-boss paths are walked. More paths => denser branching.</summary>
        public int PathCount = 6;

        /// <summary>Chance (0..1) a middle-row combat node is upgraded to an Elite.</summary>
        public float EliteChance = 0.16f;

        /// <summary>Chance (0..1) a middle-row node becomes a Shop. Never two shops in a row of a path.</summary>
        public float ShopChance = 0.12f;

        /// <summary>Elites are not allowed below this row (early runs stay easy).</summary>
        public int MinEliteRow = 3;

        /// <summary>Chance (0..1) a middle-row node becomes a Campfire (rest / random upgrade).</summary>
        public float CampfireChance = 0.10f;
    }
}
