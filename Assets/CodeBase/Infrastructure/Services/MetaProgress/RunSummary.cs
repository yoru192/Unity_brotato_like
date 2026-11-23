namespace CodeBase.Infrastructure.Services.MetaProgress
{
    /// <summary>Підсумок завершеного забігу для екранів Win / Game Over.</summary>
    public readonly struct RunSummary
    {
        public readonly int NodesCompleted;
        public readonly int EssenceReward;
        public readonly int EssenceTotal;

        /// <summary>Забіг пройшов більше вузлів, ніж будь-який попередній.</summary>
        public readonly bool IsNewBest;

        public RunSummary(int nodesCompleted, int essenceReward, int essenceTotal, bool isNewBest)
        {
            NodesCompleted = nodesCompleted;
            EssenceReward = essenceReward;
            EssenceTotal = essenceTotal;
            IsNewBest = isNewBest;
        }
    }
}
