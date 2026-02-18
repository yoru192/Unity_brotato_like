#if UNITY_EDITOR
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor
{
    public class WaveControllerWindow : EditorWindow
    {
        private WaveControllerStaticData _soData;
        private WaveController _runtimeController;
        private EnemySpawner _runtimeSpawner;
        private SerializedObject _serializedSo;
        private Vector2 _scrollPos;


        [MenuItem("Tools/Wave Controller")]
        public static void Open()
        {
            var window = GetWindow<WaveControllerWindow>("Wave Controller");
            window.minSize = new Vector2(420, 500);
            window.Show();
        }

        private void OnEnable()
        {
            TryAutoFindSo();
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawSoSelector();
            EditorGUILayout.Space(6);

            if (_soData == null)
            {
                EditorGUILayout.HelpBox("Перетягни або вибери WaveControllerStaticData asset.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                return;
            }

            DrawStaticDataEditor();
            EditorGUILayout.Space(8);
            DrawRuntimeSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSoSelector()
        {
            EditorGUILayout.LabelField("📁 Static Data Asset", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUI.BeginChangeCheck();
            var newSo = (WaveControllerStaticData)EditorGUILayout.ObjectField(
                _soData, typeof(WaveControllerStaticData), false);
            if (EditorGUI.EndChangeCheck())
                SetSo(newSo);

            if (GUILayout.Button("Auto Find", GUILayout.Width(80)))
                TryAutoFindSo();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStaticDataEditor()
        {
            EditorGUILayout.LabelField("⚙️ Wave Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            _serializedSo.Update();

            DrawLabeledField("Wave Value Multiplier", "waveValueMultiplier");
            DrawLabeledField("Max Enemies Per Wave",  "maxEnemiesPerWave");
            DrawLabeledField("Initial Spawn Timer (s)",   "initialSpawnTimer");
            DrawLabeledField("Wave Duration (s)",     "waveDuration");
            DrawLabeledField("Spawn Interval (s)",    "spawnInterval");
            DrawLabeledField("Max Alive Enemies",     "maxAlive");

            EditorGUILayout.Space(6);
            DrawEnemyConfigsTable();

            if (_serializedSo.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(_soData);
                _runtimeController?.Construct(_soData);
            }

            EditorGUILayout.EndVertical();
        }
        
        private void DrawEnemyConfigsTable()
        {
            EditorGUILayout.LabelField("Enemy Configs", EditorStyles.boldLabel);

            SerializedProperty configsProp = _serializedSo.FindProperty("enemyConfigs");
            
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("#",        EditorStyles.miniLabel, GUILayout.Width(20));
            GUILayout.Label("Type",     EditorStyles.miniLabel, GUILayout.Width(110));
            GUILayout.Label("Cost",     EditorStyles.miniLabel, GUILayout.Width(50));
            GUILayout.Label("",                                 GUILayout.Width(22));
            EditorGUILayout.EndHorizontal();
            
            for (int i = 0; i < configsProp.arraySize; i++)
            {
                SerializedProperty element  = configsProp.GetArrayElementAtIndex(i);
                SerializedProperty typeProp = element.FindPropertyRelative("enemyTypeId");
                SerializedProperty costProp = element.FindPropertyRelative("cost");

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                GUILayout.Label(i.ToString(), EditorStyles.miniLabel, GUILayout.Width(20));
                EditorGUILayout.PropertyField(typeProp, GUIContent.none, GUILayout.Width(110));
                EditorGUILayout.PropertyField(costProp, GUIContent.none, GUILayout.Width(50));

                GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("✕", GUILayout.Width(22)))
                    configsProp.DeleteArrayElementAtIndex(i);
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
            if (GUILayout.Button("+ Add Enemy Type", GUILayout.Width(140)))
                configsProp.InsertArrayElementAtIndex(configsProp.arraySize);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawLabeledField(string label, string propertyName)
        {
            EditorGUILayout.PropertyField(
                _serializedSo.FindProperty(propertyName),
                new GUIContent(label));
        }

        private void DrawRuntimeSection()
        {
            EditorGUILayout.LabelField("🎮 Runtime Controls", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Запусти Play Mode для керування хвилями.", MessageType.Info);
                return;
            }
            
            if (_runtimeController == null)
                _runtimeController = FindFirstObjectByType<WaveController>();
            if(_runtimeSpawner == null)
                _runtimeSpawner = FindFirstObjectByType<EnemySpawner>();

            if (_runtimeController == null)
            {
                EditorGUILayout.HelpBox("WaveController не знайдено на сцені.", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🔴 Live Status", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Current Wave",  _runtimeController.currentWave);
                EditorGUILayout.IntField("Wave Budget",   _runtimeController.waveBudget);
            }
            
            

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);
            
            EditorGUILayout.BeginHorizontal();
            

            GUI.backgroundColor = new Color(0.4f, 0.8f, 1f);
            if (GUILayout.Button("⏭ Next Wave", GUILayout.Height(30)))
            {
                _runtimeController.currentWave++;
                _runtimeController.GenerateWave();
            }

            GUI.backgroundColor = new Color(1f, 0.8f, 0.3f);
            if (GUILayout.Button("↺ Restart Wave", GUILayout.Height(30)))
            {
                _runtimeSpawner.DestroyAllEnemies();
                _runtimeController.GenerateWave();
            }
               

            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
            if (GUILayout.Button("▶ Wave 1", GUILayout.Height(30)))
            {
                _runtimeController.currentWave = 1;
                _runtimeController.GenerateWave();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Jump to Wave:", GUILayout.Width(100));
            _jumpToWave = EditorGUILayout.IntField(_jumpToWave, GUILayout.Width(60));
            _jumpToWave = Mathf.Max(0, _jumpToWave);

            GUI.backgroundColor = new Color(0.8f, 0.5f, 1f);
            if (GUILayout.Button("Go", GUILayout.Width(40)))
            {
                _runtimeController.currentWave = _jumpToWave;
                _runtimeController.GenerateWave();
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.EndHorizontal();
            
            Repaint();
        }

        private int _jumpToWave;


        private void SetSo(WaveControllerStaticData so)
        {
            _soData = so;
            _serializedSo = _soData != null ? new SerializedObject(_soData) : null;
        }

        private void TryAutoFindSo()
        {
            string[] guids = AssetDatabase.FindAssets("t:WaveControllerStaticData");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                SetSo(AssetDatabase.LoadAssetAtPath<WaveControllerStaticData>(path));
            }
        }
    }
}
#endif
