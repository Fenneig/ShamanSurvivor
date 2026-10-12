using System.Collections.Generic;
using ShamanSurvivor.Runtime;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ShamanSurvivor.Presentation
{
    public sealed class EarthWavePresenter : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private EarthWaveSegmentView _segmentPrefab;
        [SerializeField, Min(0)] private int _prewarmCount = 96;
        [Header("Trail")]
        [SerializeField, Min(0.1f)] private float _rowSpacing = 0.65f;
        [SerializeField, Min(0.1f)] private float _sideSegmentSpacing = 1f;
        [SerializeField, Range(1, 4)] private int _maxSidePairs = 3;
        [SerializeField, Range(1, 32)] private int _maxRowsPerFrame = 12;
        [Header("Segment")]
        [SerializeField] private float _segmentLifetime = 0.35f;
        [SerializeField] private float _mainScale = 1f;
        [SerializeField] private float _sideScale = 0.75f;
        [Header("World")]
        [SerializeField] private float _zPosition = -0.15f;
       
        [Header("Visual Size Progression")]
        [SerializeField, Min(0f)]
        private float _forwardOffsetPer100Percent = 1f;
        
        [Header("Debug Hitbox")]
        [SerializeField]
        private EarthWaveHitboxDebugView _debugPrefab;
        [SerializeField]
        private float _debugZPosition = -0.05f;
        private struct TrailState
        {
            public Vector2 LastPosition;
            public float DistanceSinceLastRow;
        }

        private World _world;
        private EntityManager _entityManager;
        private EntityQuery _waveQuery;
        private EntityQuery _flowQuery;

        private readonly Dictionary<Entity, TrailState> _tracks = new();
        private readonly HashSet<Entity> _seen = new();
        private readonly List<Entity> _expiredTracks = new();
        private readonly Queue<EarthWaveSegmentView> _available = new();
        private readonly List<EarthWaveSegmentView> _activeSegments = new();
        
        private readonly Dictionary<Entity, EarthWaveHitboxDebugView> _debugViews = new();
        private readonly Queue<EarthWaveHitboxDebugView> _debugAvailable = new();
        private readonly List<Entity> _expiredDebug = new();

        private void Awake()
        {
            for (int i = 0; i < _prewarmCount; i++)
                _available.Enqueue(CreateView());
        }

        private void LateUpdate()
        {
            if (!TryInitialize())
                return;

            if (!IsGameplayRunning())
                return;

            float deltaTime = Time.deltaTime;

            UpdateSegments(deltaTime);
            UpdateWaves();
        }

        private void UpdateWaves()
        {
            _seen.Clear();

            using NativeArray<Entity> entities = _waveQuery.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];

                _seen.Add(entity);

                Projectile projectile = _entityManager.GetComponentData<Projectile>(entity);
                EarthWave wave = _entityManager.GetComponentData<EarthWave>(entity);
                ProjectileModifierSnapshot snapshot = _entityManager.GetComponentData<ProjectileModifierSnapshot>(entity);
                Unity.Transforms.LocalTransform transform = _entityManager.GetComponentData<Unity.Transforms.LocalTransform>(entity);
                Vector2 position = new Vector2(transform.Position.x, transform.Position.y);
                Vector2 direction = new Vector2(projectile.Direction.x, projectile.Direction.y);
                float sizeMultiplier = snapshot.SizeMultiplier;
                float sizeBonus = Mathf.Max(0f, sizeMultiplier - 1f);
                float forwardOffset = sizeBonus * _forwardOffsetPer100Percent;
                Vector2 visualOffset = direction * forwardOffset;
                
                if (direction.sqrMagnitude < 0.0001f)
                    continue;

                direction.Normalize();

                float halfWidth = wave.HalfWidth * snapshot.SizeMultiplier;

                float halfDepth = wave.HalfDepth * snapshot.SizeMultiplier;

                SyncDebugHitbox(entity, position, direction, halfWidth, halfDepth);
                
                if (!_tracks.TryGetValue(entity, out TrailState trail))
                {
                    SpawnRow(position  + visualOffset, direction, halfWidth, sizeMultiplier);

                    trail = new TrailState
                    {
                        LastPosition = position,
                        DistanceSinceLastRow = 0f
                    };

                    _tracks.Add(entity, trail);

                    continue;
                }

                Vector2 travel = position - trail.LastPosition;

                float distance = travel.magnitude;

                if (distance > 0.0001f)
                {
                    float nextRowDistance = _rowSpacing - trail.DistanceSinceLastRow;
                    int rowsCreated = 0;
                    
                    while (nextRowDistance <= distance &&
                           rowsCreated < _maxRowsPerFrame)
                    {
                        float t = nextRowDistance / distance;
                        Vector2 rowPosition = Vector2.Lerp(trail.LastPosition, position, t);
                        SpawnRow(rowPosition  + visualOffset, direction, halfWidth, sizeMultiplier);
                        nextRowDistance += _rowSpacing;
                        rowsCreated++;
                    }

                    trail.DistanceSinceLastRow = Mathf.Repeat(trail.DistanceSinceLastRow + distance, _rowSpacing);
                }

                trail.LastPosition = position;
                _tracks[entity] = trail;
            }

            _expiredTracks.Clear();

            foreach (var pair in _tracks)
            {
                if (!_seen.Contains(pair.Key))
                    _expiredTracks.Add(pair.Key);
            }
            
            ReleaseExpiredDebugHitboxes();
            
            foreach (Entity entity in _expiredTracks)
                _tracks.Remove(entity);
        }

        private void SpawnRow(Vector2 position, Vector2 direction, float halfWidth, float sizeMultiplier)     
        {
            Vector2 right = new Vector2(-direction.y, direction.x);

            int sidePairs = Mathf.Clamp(Mathf.CeilToInt(halfWidth / _sideSegmentSpacing), 1, _maxSidePairs);

            for (int i = -sidePairs; i <= sidePairs; i++)
            {
                bool isMain = i == 0;

                float normalizedOffset = i / (float)sidePairs;

                Vector2 segmentPosition = position + right * (normalizedOffset * halfWidth * 0.75f);

                if (!isMain)
                {
                    float stagger = Mathf.Abs(i) % 2 == 0
                            ? -0.10f
                            : 0.10f;

                    segmentPosition += direction * stagger;
                }

                EarthWaveSegmentView view = GetView();
                view.Play(segmentPosition, direction, isMain, isMain ? _mainScale : _sideScale, sizeMultiplier, _segmentLifetime, _zPosition);            
                _activeSegments.Add(view);
            }
        }

        private void UpdateSegments(float deltaTime)
        {
            for (int i = _activeSegments.Count - 1; i >= 0; i--)
            {
                EarthWaveSegmentView view = _activeSegments[i];

                if (view.Tick(deltaTime))
                    continue;

                view.Hide();
                _available.Enqueue(view);
                _activeSegments.RemoveAt(i);
            }
        }

        private EarthWaveSegmentView GetView()
        {
            if (_available.Count > 0)
                return _available.Dequeue();

            return CreateView();
        }

        private EarthWaveSegmentView CreateView()
        {
            EarthWaveSegmentView view = Instantiate(_segmentPrefab, transform);
            view.gameObject.SetActive(false);

            return view;
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

            _waveQuery = _entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<EarthWave>(),
                ComponentType.ReadOnly<Projectile>(),
                ComponentType.ReadOnly<ProjectileModifierSnapshot>(),
                ComponentType.ReadOnly<Unity.Transforms.LocalTransform>());

            _flowQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<GameFlowState>());

            return true;
        }

        private bool IsGameplayRunning()
        {
            if (_flowQuery.IsEmptyIgnoreFilter)
                return true;

            GameFlowState flow = _flowQuery.GetSingleton<GameFlowState>();

            return flow.Phase == GamePhase.Playing;
        }

        private void ReleaseAll()
        {
            foreach (EarthWaveSegmentView view in _activeSegments)
            {
                view.Hide();
                _available.Enqueue(view);
            }
            
            foreach (var view in _debugViews.Values)
            {
                view.Hide();
                _debugAvailable.Enqueue(view);
            }

            _debugViews.Clear();
            _expiredDebug.Clear();

            _activeSegments.Clear();
            _tracks.Clear();
            _seen.Clear();
            _expiredTracks.Clear();
        }

        private void OnDisable()
        {
            ReleaseAll();
        }
        
        private void SyncDebugHitbox(Entity entity, Vector2 position, Vector2 direction, float halfWidth, float halfDepth)
        {
            if (_debugPrefab == null || !_debugPrefab.EnabledInPrefab)
                return;

            if (!_debugViews.TryGetValue(entity, out EarthWaveHitboxDebugView view))
            {
                view = _debugAvailable.Count > 0 ? _debugAvailable.Dequeue() : Instantiate(_debugPrefab, transform);
                view.gameObject.SetActive(true);
                _debugViews.Add(entity, view);
            }
            
            view.Sync(new float2(position.x, position.y), new float2(direction.x, direction.y), halfWidth, halfDepth, _debugZPosition);
        }
        
        private void ReleaseExpiredDebugHitboxes()
        {
            _expiredDebug.Clear();

            foreach (var pair in _debugViews)
                if (!_seen.Contains(pair.Key)) 
                    _expiredDebug.Add(pair.Key);

            foreach (Entity entity in _expiredDebug)
            {
                EarthWaveHitboxDebugView view = _debugViews[entity];

                view.Hide();
                _debugAvailable.Enqueue(view);
                _debugViews.Remove(entity);
            }
        }
    }
}