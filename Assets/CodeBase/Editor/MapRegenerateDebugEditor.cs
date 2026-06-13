using CodeBase.Infrastructure.Map;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor
{
    [CustomEditor(typeof(MapRegenerateDebug))]
    public class MapRegenerateDebugEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8);

            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("🗺  Regenerate Map", GUILayout.Height(36)))
            {
                ((MapRegenerateDebug)target).RegenerateMap();
            }

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("Запусти гру щоб активувати кнопку.", MessageType.Info);

            GUI.enabled = true;
        }
    }
}