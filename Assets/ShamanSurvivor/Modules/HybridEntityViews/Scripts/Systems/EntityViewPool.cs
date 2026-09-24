using System.Collections.Generic;
using UnityEngine;

namespace Unity.Entities.HybridViews.Modules.HybridEntityViews
{
    /// <summary>
    /// Internal pool for <see cref="EntityView"/> instances.
    /// </summary>
    internal sealed class EntityViewPool : MonoBehaviour
    {
        private readonly Dictionary<EntityView, Pool> _pools = new();

        internal EntityView Rent(EntityView prefab)
        {
            if (!_pools.TryGetValue(prefab, out Pool pool))
            {
                pool = CreatePool(prefab);
                _pools.Add(prefab, pool);
            }

            if (pool.Inactive.Count == 0)
                Grow(pool, prefab);

            EntityView view = pool.Inactive.Pop();

            view.transform.SetParent(pool.ActiveTransform, false);
            view.gameObject.SetActive(true);

            return view;
        }

        internal void Return(EntityView view)
        {
            if (view == null)
                return;

            if (!_pools.TryGetValue(view.prefab, out Pool pool))
            {
                Destroy(view.gameObject);
                return;
            }

            view.gameObject.SetActive(false);
            view.transform.SetParent(pool.InactiveTransform, false);

            pool.Inactive.Push(view);
        }

        private Pool CreatePool(EntityView prefab)
        {
            GameObject root = new($"Pool [{prefab.name}]");
            root.transform.SetParent(transform, false);

            Transform active = new GameObject("Active").transform;
            active.SetParent(root.transform, false);

            Transform inactive = new GameObject("Inactive").transform;
            inactive.SetParent(root.transform, false);

            Pool pool = new(active, inactive);

            CreateInstance(pool, prefab);

            return pool;
        }

        private static void Grow(Pool pool, EntityView prefab)
        {
            int growBy = pool.Count;

            for (int i = 0; i < growBy; i++)
                CreateInstance(pool, prefab);
        }

        private static void CreateInstance(Pool pool, EntityView prefab)
        {
            EntityView view = Instantiate(prefab, pool.InactiveTransform);

            view.prefab = prefab;
            view.gameObject.SetActive(false);

            pool.Inactive.Push(view);
            pool.Count++;
        }

        private sealed class Pool
        {
            public readonly Transform ActiveTransform;
            public readonly Transform InactiveTransform;

            public readonly Stack<EntityView> Inactive = new(1);

            /// <summary>
            /// Total number of instances created for this pool.
            /// </summary>
            public int Count;

            public Pool(
                Transform activeTransform,
                Transform inactiveTransform)
            {
                ActiveTransform = activeTransform;
                InactiveTransform = inactiveTransform;
            }
        }
    }
}