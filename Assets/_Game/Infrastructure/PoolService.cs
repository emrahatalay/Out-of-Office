using System;
using System.Collections.Generic;
using OutOfOffice.Core.Services;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OutOfOffice.Infrastructure
{
    /// <summary>Prefab başına basit nesne havuzu. Mobilde Instantiate/Destroy takılmalarını önler.</summary>
    public sealed class PoolService : IPoolService, IDisposable
    {
        readonly Dictionary<GameObject, Stack<GameObject>> _free = new Dictionary<GameObject, Stack<GameObject>>();
        readonly Dictionary<GameObject, GameObject> _prefabOf = new Dictionary<GameObject, GameObject>();
        readonly Transform _root;

        public PoolService()
        {
            _root = new GameObject("[Pool]").transform;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject instance = null;
            if (_free.TryGetValue(prefab, out var stack))
            {
                while (stack.Count > 0 && instance == null) instance = stack.Pop();
            }

            if (instance == null)
            {
                instance = Object.Instantiate(prefab, position, rotation, _root);
                _prefabOf[instance] = prefab;
            }
            else
            {
                instance.transform.SetPositionAndRotation(position, rotation);
            }

            instance.SetActive(true);
            return instance;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null) return;

            if (!_prefabOf.TryGetValue(instance, out var prefab))
            {
                Object.Destroy(instance);
                return;
            }

            instance.SetActive(false);
            if (!_free.TryGetValue(prefab, out var stack))
            {
                stack = new Stack<GameObject>();
                _free.Add(prefab, stack);
            }

            stack.Push(instance);
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0) return;

            if (!_free.TryGetValue(prefab, out var stack))
            {
                stack = new Stack<GameObject>(count);
                _free.Add(prefab, stack);
            }

            for (int i = stack.Count; i < count; i++)
            {
                var instance = Object.Instantiate(prefab, _root);
                _prefabOf[instance] = prefab;
                instance.SetActive(false);
                stack.Push(instance);
            }
        }

        public void Dispose()
        {
            if (_root != null) Object.Destroy(_root.gameObject);
            _free.Clear();
            _prefabOf.Clear();
        }
    }
}
