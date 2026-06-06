using CodeBase.Infrastructure.Map;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.Services.SelectedLevel
{
    /// <summary>
    /// Carries the map node chosen on the level map into the gameplay scene, so the spawner
    /// can be built with that node's wave configuration instead of the single global one.
    /// </summary>
    public interface ISelectedLevelService : IService
    {
        MapNode SelectedNode { get; }
        WaveControllerStaticData SelectedWaveConfig { get; }

        void Select(MapNode node, WaveControllerStaticData waveConfig);
        void Clear();
    }
}
