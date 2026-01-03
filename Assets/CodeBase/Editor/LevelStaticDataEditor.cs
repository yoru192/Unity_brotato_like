using System.Linq;
using CodeBase.StaticData;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodeBase.Editor
{
    [CustomEditor(typeof(LevelStaticData))]
    public class LevelStaticDataEditor : UnityEditor.Editor
    {
        private const string SpawnPointTag = "SpawnPoint";
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            LevelStaticData levelData = (LevelStaticData)target;

            if (GUILayout.Button("Collect from Scene"))
            {
                // Збираємо позиції спавнерів
                GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(SpawnPointTag);
                levelData.spawnersPosition = spawnPoints
                    .Select(go => (Vector2)go.transform.position)
                    .ToList();
                
                Debug.Log($"Collected: {levelData.spawnersPosition.Count} spawn points");
                EditorUtility.SetDirty(target);
            }
        }
    }
}