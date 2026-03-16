using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace CodeBase.Logic
{
    public class ObjectPoolManager : MonoBehaviour
    {
        [SerializeField] private bool addToDontDestroyOnLoad;

        private GameObject _emptyHolder;
        
        public static bool HasPool(GameObject prefab) => _objectPools.ContainsKey(prefab);
        
        private static GameObject _enemyObjectsEmpty;
        private static GameObject _projectileObjectsEmpty;

        private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPools;
        private static Dictionary<GameObject, GameObject> _cloneToPrefabMap;
        
        public enum PoolType
        {
            Enemy,
            Projectile,
        }
        public static PoolType PoolingType;

        public void Awake()
        {
            _objectPools  = new Dictionary<GameObject, ObjectPool<GameObject>>();
            _cloneToPrefabMap  = new Dictionary<GameObject, GameObject>();

            SetupEmpties();
        }

        private void SetupEmpties()
        {
           _emptyHolder = new GameObject("Object Pools");

           _enemyObjectsEmpty = new GameObject("Enemies");
           _enemyObjectsEmpty.transform.SetParent(_emptyHolder.transform);
           
           _projectileObjectsEmpty = new GameObject("Projectiles");
           _projectileObjectsEmpty.transform.SetParent(_emptyHolder.transform);
           
           if (addToDontDestroyOnLoad)
               DontDestroyOnLoad(_projectileObjectsEmpty.transform.root);
        }

        private static void CreatePool(GameObject prefab, Vector2 pos, Quaternion rot, PoolType poolType)
        {
            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => CreateObject(prefab, pos, rot, poolType),
                actionOnGet: OnGetObject,
                actionOnRelease: OnReleaseObject,
                actionOnDestroy: OnDestroyObject
            );
            
            _objectPools.Add(prefab, pool);
        }
        
        private static GameObject CreateObject(GameObject prefab, Vector2 pos, Quaternion rot, PoolType poolType)
        {
            prefab.SetActive(false);
            
            GameObject obj = Instantiate(prefab, pos, rot);
            
            prefab.SetActive(true);
            
            GameObject parentObject = SetParentObject(poolType);
            obj.transform.SetParent(parentObject.transform);
            
            return obj;
        }

        private static void OnGetObject(GameObject obj)
        {
            
        }

        private static void OnReleaseObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        private static void OnDestroyObject(GameObject obj)
        {
            if (_cloneToPrefabMap.ContainsKey(obj))
            {
                _cloneToPrefabMap.Remove(obj);
            }
        }

        private static GameObject SetParentObject(PoolType poolType)
        {
            switch (poolType)
            {
                case PoolType.Projectile:
                    return _projectileObjectsEmpty;
                case PoolType.Enemy:
                    return _enemyObjectsEmpty;
                default: 
                    return null;
            }
        }

        private static T SpawnObject<T>(GameObject objectToSpawn, Vector2 spawnPos, Quaternion spawnRotation, PoolType poolType) where T : Object
        {
            if (!_objectPools.ContainsKey(objectToSpawn))
            {
                CreatePool(objectToSpawn, spawnPos, spawnRotation, poolType);
            }

            GameObject obj = _objectPools[objectToSpawn].Get();

            if (obj != null)
            {
                _cloneToPrefabMap.TryAdd(obj, objectToSpawn);
                
                obj.transform.position = spawnPos;
                obj.transform.rotation = spawnRotation;
                obj.SetActive(true);

                if (typeof(T) == typeof(GameObject))
                {
                    return obj as T;
                }
                
                T component = obj.GetComponent<T>();
                if (component == null)
                {
                    Debug.LogError($"Object {objectToSpawn.name} has no component of type {typeof(T)}");
                    return null;
                }
                return component;
            }
            return null;
        }

        public static T SpawnObject<T>(T typePrefab, Vector2 spawnPos, Quaternion spawnRotation, PoolType poolType) where T : Component
        {
            return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRotation, poolType);
        }
        
        public static GameObject SpawnObject(GameObject objectToSpawn, Vector2 spawnPos, Quaternion spawnRotation, PoolType poolType)
        {
            return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
        }

        public static void ReturnObjectToPool(GameObject obj, PoolType poolType)
        {
            if (_cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
            {
                GameObject parentObject = SetParentObject(poolType);

                if (obj.transform.parent != parentObject.transform)
                {
                    obj.transform.SetParent(parentObject.transform);
                }

                if (_objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
                {
                    pool.Release(obj);
                }
            }
            else
            {
                Debug.LogWarning($"Trying to return an object that is not pooled: {obj.name}");
            }
        }
    }
}
