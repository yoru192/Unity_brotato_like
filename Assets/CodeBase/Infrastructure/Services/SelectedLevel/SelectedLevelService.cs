using CodeBase.Infrastructure.Map;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.Services.SelectedLevel
{
    public class SelectedLevelService : ISelectedLevelService
    {
        public MapNode SelectedNode { get; private set; }
        public WaveControllerStaticData SelectedWaveConfig { get; private set; }

        public void Select(MapNode node, WaveControllerStaticData waveConfig)
        {
            SelectedNode = node;
            SelectedWaveConfig = waveConfig;
        }

        public void Clear()
        {
            SelectedNode = null;
            SelectedWaveConfig = null;
        }
    }
}
