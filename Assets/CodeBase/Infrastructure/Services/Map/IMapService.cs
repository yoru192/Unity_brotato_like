using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Map;

namespace CodeBase.Infrastructure.Services.Map
{
    /// <summary>
    /// Owns the runtime branching map: generation from a persisted seed, the set of currently
    /// reachable nodes, and progression as the player completes runs.
    /// </summary>
    public interface IMapService : IService
    {
        GeneratedMap CurrentMap { get; }

        /// <summary>Raised when node states change (completion, regeneration) so an open map UI can refresh.</summary>
        event Action Changed;

        /// <summary>Builds the map from saved progress, or generates a fresh one if none exists.</summary>
        void EnsureMap();

        /// <summary>Discards the current map and generates a brand new one with a random seed.</summary>
        void RegenerateMap();

        IReadOnlyList<MapNode> AvailableNodes { get; }

        /// <summary>Picks an available node, wiring its wave config for the upcoming run.</summary>
        bool SelectNode(MapNode node);

        /// <summary>Marks the node played this run as completed and opens its successors.</summary>
        void CompleteSelectedNode();

        /// <summary>True once a boss node has been completed.</summary>
        bool IsMapCompleted { get; }
    }
}
