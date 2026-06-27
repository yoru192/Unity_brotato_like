using UnityEditor;
using UnityEngine;

namespace BerserkPixel.Tilemap_Generator
{
    [CustomEditor(typeof(LevelGenerator))]
    public class LevelGeneratorEditor: Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            LevelGenerator generator = (LevelGenerator)target;
            
            AddGUI(generator);
        }

        private void AddGUI(LevelGenerator generator)
        {
            GUILayout.Space(16);
            
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Map"))
            {
                Debug.Log("Generating Map");
                generator.GenerateLayers();
            }
            
            if (GUILayout.Button("Save Map"))
            {
                Debug.Log("Saving Map");
                generator.SaveAssetMap();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.Space(8);

            if (!GUILayout.Button("Clear Map")) return;
            
            Debug.Log("Clearing Map");
            generator.ClearMap();
        }
    }
}