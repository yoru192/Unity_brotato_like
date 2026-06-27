using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField] private string mapName;

        [SerializeField] private GameObject objectToSave;
        
        [Tooltip("the list of layers that will be used to generate the tilemap")]
        [SerializeField] private List<MapLayer> layers;

    #if UNITY_EDITOR
        
        public void GenerateLayers()
        {
            foreach (var layer in layers)
            {
                if (layer.active)
                {
                    Debug.Log($"Generating {layer.name}");
                    layer.GenerateMap();
                }
            }

            foreach (var shadow in FindObjectsOfType<TilemapShadow>())
            {
                shadow.UpdateShadows();
            }
        }

        public void ClearMap()
        {
            foreach (var layer in layers)
            {
                if (layer.active)
                {
                    layer.ClearMap();
                }
            }

            foreach (var shadow in FindObjectsOfType<TilemapShadow>())
            {
                shadow.ClearShadows();
            }            
        }

        /// <summary>
        /// Saves a map on a Prefab using the DirectoryHelpers script
        /// </summary>
        public void SaveAssetMap()
        {
            // choose a random name if there's nothing in the inspector 
            var saveName = mapName.Equals("") ? 
                MapGeneratorConst.WorldNames[Random.Range(0, MapGeneratorConst.WorldNames.Length)] : 
                mapName;

            // we need to wrap the saved game object with a grid component if it's not there
            
            DirectoryHelpers.SaveMap(
                saveName: saveName,
                grid: objectToSave,
                onError: (error) =>
                {
                    EditorUtility.DisplayDialog("Tilemap NOT saved", error, "Continue");
                },
                onSuccess: (message) =>
                {
                    EditorUtility.DisplayDialog("Tilemap saved", message, "Continue");
                    mapName = "";
                }
            );
        }
        
    #endif

    }
}