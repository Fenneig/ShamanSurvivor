using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Code.Runtime
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject _prefab;
        [Header("Spawn")]
        [SerializeField] private float _spawnRate = 20f;
        [SerializeField] private float _minSpawnRadius = 7f;
        [SerializeField] private float _maxSpawnRadius = 10f;
        [Min(1)]
        [SerializeField] private int _maxAlive = 200;
        [Header("Random")]
        [Min(1)]
        [SerializeField] private uint _randomState = 1;

        public class EnemySpawnerBaker : Baker<EnemySpawnerAuthoring>
        {
            public override void Bake(EnemySpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new EnemySpawner
                {
                    Prefab = GetEntity(authoring._prefab, TransformUsageFlags.Dynamic),
                    SpawnRate = authoring._spawnRate,
                    MinSpawnRadius = authoring._minSpawnRadius,
                    MaxSpawnRadius = authoring._maxSpawnRadius,
                    MaxAlive = authoring._maxAlive,
                    SpawnAccumulator = 0f,
                    RandomState = authoring._randomState
                });
            }
        }
    }
}