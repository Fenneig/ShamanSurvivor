using System.Collections.Generic;
using ShamanSurvivor.Runtime;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class LightningShieldPresenter : MonoBehaviour
    {
        [Header("Prefab")] 
        [SerializeField] private LightningOrbView _orbPrefab;
        [Header("Pool")]
        [SerializeField, Min(0)] private int _prewarmCount = 8;
        [Header("World")]
        [SerializeField] private float _zPosition = -0.15f;
        [Header("Visual Scale")]
        [SerializeField] private float _diameterMultiplierFromHitRadius = 2f;

        private readonly Dictionary<Entity, LightningOrbView> _active = new();
        private readonly Stack<LightningOrbView> _pool = new();
        private readonly HashSet<Entity> _seen = new();
        private readonly List<Entity> _toRelease = new();

        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _orbQuery;

        private void Awake()
        {
            if (_orbPrefab == null)
                return;

            for (int i = 0; i < _prewarmCount; i++)
            {
                LightningOrbView view = Instantiate(_orbPrefab, transform);
                view.gameObject.SetActive(false);
                _pool.Push(view);
            }
        }

        private void LateUpdate()
        {
            if (!TryInitialize())
            {
                ReleaseAll();
                return;
            }

            float deltaTime = Time.deltaTime;

            _seen.Clear();

            using NativeArray<Entity> entities = _orbQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                LightningOrb orb = _entityManager.GetComponentData<LightningOrb>(entity);
                LocalTransform transform = _entityManager.GetComponentData<LocalTransform>(entity);
                Vector2 position = new Vector2(transform.Position.x, transform.Position.y);
                LightningOrbView view = GetOrCreate(entity);

                float diameter = orb.HitRadius * _diameterMultiplierFromHitRadius;

                view.Show(position, diameter, _zPosition);
                view.Tick(deltaTime);

                _seen.Add(entity);
            }

            ReleaseMissing();
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

            _world = world;
            _entityManager = world.EntityManager;

            _orbQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<LightningOrb>(),
                ComponentType.ReadOnly<LightningOrbActive>(),
                ComponentType.ReadOnly<LocalTransform>());

            return true;
        }

        private LightningOrbView GetOrCreate(Entity entity)
        {
            if (_active.TryGetValue(entity, out LightningOrbView existing))
                return existing;

            var view = _pool.Count > 0 ? _pool.Pop() : Instantiate(_orbPrefab, transform);

            view.gameObject.SetActive(true);
            _active.Add(entity, view);

            return view;
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
                LightningOrbView view = _active[entity];

                view.Hide();
                _pool.Push(view);

                _active.Remove(entity);
            }
        }

        private void ReleaseAll()
        {
            foreach (var pair in _active)
            {
                pair.Value.Hide();
                _pool.Push(pair.Value);
            }

            _active.Clear();
            _seen.Clear();
            _toRelease.Clear();
        }

        private void OnDisable()
        {
            ReleaseAll();
        }
    }
}