using UnityEditor;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator
{
    [CustomEditor(typeof(MapObjectPlacer))]
    public class MapObjectPlacerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapObjectPlacer placer = (MapObjectPlacer)target;
            
            if (DrawDefaultInspector() && placer.autoUpdate)
            {
                placer.ClearObjects();
                placer.PlaceObjects();
            }
            
            GUILayout.Space(8);
            
            if (GUILayout.Button("Place Objects"))
            {
                Debug.Log("Placing Objects");
                placer.PlaceObjects();
            }

            GUILayout.Space(8);
            
            if (GUILayout.Button("Clear Objects"))
            {
                Debug.Log("Removing Objects");
                placer.ClearObjects();
            }
        }
    }
}