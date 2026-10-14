using System.Collections.Generic;
using ShamanSurvivor.Runtime;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class LightningShieldDebugPresenter : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private LightningOrbDebugView _orbPrefab;
        [Header("Orbit")]
        [SerializeField] private LineRenderer _orbitCircle;
        [SerializeField, Range(32, 256)] private int _orbitSegments = 96;
        [Header("Debug")]
        [SerializeField] private bool _showHitboxes = true;
        [SerializeField] private bool _showOrbit = true;
        [SerializeField] private float _zPosition = -0.15f;
        [Header("Pool")]
        [SerializeField, Min(0)] private int _prewarmCount = 8;

        private readonly Dictionary<Entity, LightningOrbDebugView> _active = new();
        private readonly Stack<LightningOrbDebugView> _pool = new();
        private readonly HashSet<Entity> _seen = new();
        private readonly List<Entity> _toRelease = new();

        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _orbQuery;

        private float _previousOrbitRadius = -1f;

        private void Awake()
        {
            if (_orbitCircle != null)
            {
                _orbitCircle.useWorldSpace = false;
                _orbitCircle.loop = true;
                _orbitCircle.gameObject.SetActive(false);
            }

            if (_orbPrefab == null)
                return;

            for (int i = 0; i < _prewarmCount; i++)
            {
                LightningOrbDebugView view = Instantiate(_orbPrefab, transform);
                view.gameObject.SetActive(false);
                _pool.Push(view);
            }
        }

        private void LateUpdate()
        {
            if (!TryInitialize())
            {
                ReleaseAll();
                HideOrbit();
                return;
            }

            if (_orbQuery.IsEmptyIgnoreFilter)
            {
                ReleaseAll();
                HideOrbit();
                return;
            }

            _seen.Clear();

            bool orbitUpdated = false;

            using NativeArray<Entity> entities = _orbQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];

                LightningOrb orb = _entityManager.GetComponentData<LightningOrb>(entity);

                LocalTransform orbTransform = _entityManager.GetComponentData<LocalTransform>(entity);

                Vector2 position = new Vector2(orbTransform.Position.x, orbTransform.Position.y);

                if (_showHitboxes && _orbPrefab != null)
                {
                    _seen.Add(entity);
                    LightningOrbDebugView view = GetOrCreate(entity);
                    view.Sync(position, orb.HitRadius, _zPosition);
                }
                
                if (!orbitUpdated && _showOrbit)
                {
                    UpdateOrbit(new Vector2(orb.ToCenter.x, orb.ToCenter.y), orb.OrbitRadius);

                    orbitUpdated = true;
                }
            }

            ReleaseMissing();

            if (!orbitUpdated)
                HideOrbit();
        }

        private bool TryInitialize()
        {
            World world = World.DefaultGameObjectInjectionWorld;

            if (world == null || !world.IsCreated)
            {
                _world = null;
                return false;
            }

            if (_world == world)
                return true;

            ReleaseAll();
            HideOrbit();

            _world = world;
            _entityManager = world.EntityManager;

            _orbQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<LightningOrb>(),
                ComponentType.ReadOnly<LightningOrbActive>(),
                ComponentType.ReadOnly<LocalTransform>());

            return true;
        }

        private LightningOrbDebugView GetOrCreate(Entity entity)
        {
            if (_active.TryGetValue(entity, out var existing))
                return existing;

            var view = _pool.Count > 0 ? _pool.Pop() :
                Instantiate(_orbPrefab, transform);

            view.gameObject.SetActive(true);

            _active.Add(entity, view);

            return view;
        }

        private void UpdateOrbit(Vector2 center, float radius)
        {
            if (_orbitCircle == null)
                return;

            if (!_orbitCircle.gameObject.activeSelf)
                _orbitCircle.gameObject.SetActive(true);

            _orbitCircle.transform.position = new Vector3(center.x, center.y, _zPosition);

            if (Mathf.Abs(radius - _previousOrbitRadius) < 0.0001f)
                return;

            _previousOrbitRadius = radius;

            LightningOrbDebugView.DrawCircle(_orbitCircle, radius, _orbitSegments);
        }

        private void HideOrbit()
        {
            if (_orbitCircle != null && _orbitCircle.gameObject.activeSelf) 
                _orbitCircle.gameObject.SetActive(false);
        }

        private void ReleaseMissing()
        {
            _toRelease.Clear();

            foreach (var pair in _active)
            {
                if (!_seen.Contains(pair.Key))
                    _toRelease.Add(pair.Key);
            }

            for (int i = 0; i < _toRelease.Count; i++)
            {
                Entity entity = _toRelease[i];

                LightningOrbDebugView view = _active[entity];

                view.gameObject.SetActive(false);
                _pool.Push(view);

                _active.Remove(entity);
            }
        }

        private void ReleaseAll()
        {
            foreach (var pair in _active)
            {
                LightningOrbDebugView view = pair.Value;

                view.gameObject.SetActive(false);
                _pool.Push(view);
            }

            _active.Clear();
            _seen.Clear();
            _toRelease.Clear();
        }

        private void OnDisable()
        {
            ReleaseAll();
            HideOrbit();
        }
    }
}