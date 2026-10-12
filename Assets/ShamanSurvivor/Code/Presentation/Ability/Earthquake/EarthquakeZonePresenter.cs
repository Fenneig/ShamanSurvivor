using System.Collections.Generic;
using ShamanSurvivor.Runtime;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class EarthquakeZonePresenter : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private EarthquakeZoneView _viewPrefab;
        [SerializeField, Min(0)] private int _prewarmCount = 8;
        [Header("World")]
        [SerializeField] private float _zPosition = -0.2f;
        
        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _zoneQuery;

        private readonly Dictionary<Entity, EarthquakeZoneView> _activeViews = new();
        private readonly Queue<EarthquakeZoneView> _available = new();
        private readonly HashSet<Entity> _seen = new();
        private readonly List<Entity> _toRelease = new();

        private void Awake()
        {
            for (int i = 0; i < _prewarmCount; i++) 
                _available.Enqueue(CreateView());
        }

        private void LateUpdate()
        {
            if (!TryInitialize())
                return;

            _seen.Clear();

            using NativeArray<Entity> entities = _zoneQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];

                EarthquakeZone zone = _entityManager.GetComponentData<EarthquakeZone>(entity);

                if (zone.RemainingDuration <= 0f)
                    continue;

                _seen.Add(entity);

                if (!_activeViews.TryGetValue(entity, out EarthquakeZoneView view))
                {
                    view = GetView();
                    _activeViews.Add(entity, view);
                }

                view.Sync(zone, _zPosition);
            }

            _toRelease.Clear();

            foreach (var pair in _activeViews)
            {
                if (!_seen.Contains(pair.Key))
                    _toRelease.Add(pair.Key);
            }

            foreach (Entity entity in _toRelease)
            {
                EarthquakeZoneView view = _activeViews[entity];

                view.Hide();
                _available.Enqueue(view);

                _activeViews.Remove(entity);
            }
        }

        private bool TryInitialize()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            if (world == null || !world.IsCreated)
            {
                if (_world != null)
                {
                    ReleaseAll();
                    _world = null;
                }

                return false;
            }

            if (_world == world)
                return true;

            ReleaseAll();

            _world = world;
            _entityManager = world.EntityManager;

            _zoneQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<EarthquakeZone>());

            return true;
        }

        private EarthquakeZoneView GetView()
        {
            if (_available.Count > 0)
                return _available.Dequeue();

            return CreateView();
        }

        private EarthquakeZoneView CreateView()
        {
            EarthquakeZoneView view = Instantiate(_viewPrefab, transform);
            view.gameObject.SetActive(false);
            return view;
        }

        private void ReleaseAll()
        {
            foreach (EarthquakeZoneView view in _activeViews.Values)
            {
                view.Hide();
                _available.Enqueue(view);
            }

            _activeViews.Clear();
            _seen.Clear();
            _toRelease.Clear();
        }

        private void OnDisable()
        {
            ReleaseAll();
        }
    }
}