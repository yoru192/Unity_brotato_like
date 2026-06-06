using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Data;
using CodeBase.Infrastructure.Map;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SelectedLevel;
using CodeBase.StaticData;
using CodeBase.StaticData.Map;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Map
{
    public class MapService : IMapService
    {
        private readonly IStaticDataService _staticData;
        private readonly IPersistentProgressService _persistentProgress;
        private readonly ISelectedLevelService _selectedLevel;

        private GeneratedMap _currentMap;

        public event Action Changed;

        public MapService(IStaticDataService staticData,
            IPersistentProgressService persistentProgress,
            ISelectedLevelService selectedLevel)
        {
            _staticData = staticData;
            _persistentProgress = persistentProgress;
            _selectedLevel = selectedLevel;
        }

        public GeneratedMap CurrentMap => _currentMap;

        private MapProgress Progress => _persistentProgress.Progress.mapProgress;

        public void EnsureMap()
        {
            if (!Progress.hasMap)
            {
                RegenerateMap();
                return;
            }

            // Rebuild if there is no in-memory map, or the cached one belongs to older progress
            // (e.g. after a restart created fresh progress with a different seed).
            if (_currentMap == null || _currentMap.Seed != Progress.seed)
                BuildFromSeed(Progress.seed);
        }

        public void RegenerateMap()
        {
            int seed = Environment.TickCount;
            Progress.hasMap = true;
            Progress.seed = seed;
            Progress.completedNodeIds.Clear();
            Progress.currentNodeId = -1;

            BuildFromSeed(seed);
            Changed?.Invoke();
        }

        public IReadOnlyList<MapNode> AvailableNodes =>
            _currentMap == null
                ? Array.Empty<MapNode>()
                : _currentMap.AllNodes.Where(node => node.State == NodeState.Available).ToList();

        public bool IsMapCompleted =>
            _currentMap != null &&
            _currentMap.AllNodes.Any(node => node.IsBoss && node.State == NodeState.Completed);

        public bool SelectNode(MapNode node)
        {
            if (node == null || node.State != NodeState.Available)
                return false;

            // Shop and Campfire nodes open an overlay and never launch a combat run, so no wave config.
            WaveControllerStaticData waveConfig = null;
            if (node.Type != NodeType.Shop && node.Type != NodeType.Campfire)
            {
                waveConfig = WaveConfigFor(node);
                if (waveConfig == null)
                    return false;
            }

            Progress.currentNodeId = node.Id;
            _selectedLevel.Select(node, waveConfig);
            return true;
        }

        public void CompleteSelectedNode()
        {
            if (_currentMap == null)
                return;

            MapNode node = _currentMap.NodeById(Progress.currentNodeId);
            if (node == null)
                return;

            if (!Progress.completedNodeIds.Contains(node.Id))
                Progress.completedNodeIds.Add(node.Id);

            Progress.currentNodeId = -1;
            _selectedLevel.Clear();

            ApplyProgressToNodeStates();
            Changed?.Invoke();
        }

        private WaveControllerStaticData WaveConfigFor(MapNode node)
        {
            MapLevelLibrary library = _staticData.GetMapLevelLibrary();
            if (library == null)
            {
                Debug.LogError("MapLevelLibrary not found in Resources/StaticData/Map");
                return null;
            }

            return library.WaveConfigFor(node, _currentMap.Seed);
        }

        private void BuildFromSeed(int seed)
        {
            MapGenerationConfig config = _staticData.GetMapSettings()?.ToGenerationConfig()
                                         ?? new MapGenerationConfig();

            _currentMap = new MapGenerator(config).Generate(seed);
            ApplyProgressToNodeStates();
        }

        /// <summary>
        /// Replays persisted completion onto the freshly generated graph: completed nodes are
        /// marked, and the reachable frontier (successors of the last completed node, or the
        /// entrances if none) becomes Available. Everything else stays Locked.
        /// </summary>
        private void ApplyProgressToNodeStates()
        {
            foreach (MapNode node in _currentMap.AllNodes)
                node.State = NodeState.Locked;

            List<int> completed = Progress.completedNodeIds;
            foreach (int id in completed)
            {
                MapNode node = _currentMap.NodeById(id);
                if (node != null)
                    node.State = NodeState.Completed;
            }

            IEnumerable<MapNode> frontier;
            if (completed.Count == 0)
            {
                frontier = _currentMap.Entrances;
            }
            else
            {
                MapNode last = _currentMap.NodeById(completed[completed.Count - 1]);
                frontier = last?.Outgoing ?? Enumerable.Empty<MapNode>();
            }

            foreach (MapNode node in frontier)
            {
                if (node.State != NodeState.Completed)
                    node.State = NodeState.Available;
            }
        }
    }
}
