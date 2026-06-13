using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Map;
using UnityEngine;

namespace CodeBase.Infrastructure.Map
{
    public class MapRegenerateDebug : MonoBehaviour
    {
        [ContextMenu("Regenerate Map")]
        public void RegenerateMap()
        {
            IMapService mapService = AllServices.Container.Single<IMapService>();
            mapService.RegenerateMap();
            Debug.Log("[Debug] Map regenerated!");
        }
    }
}