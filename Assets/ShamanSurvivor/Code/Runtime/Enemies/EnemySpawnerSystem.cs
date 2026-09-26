using ShamanSurvivor.Code.Runtime.Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace ShamanSurvivor.Code.Runtime
{
    [UpdateInGroup(typeof(GameSpawnSystemGroup))]
    public partial struct EnemySpawnerSystem : ISystem
    {
        private EntityQuery _enemyQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<EnemySpawner>();

            _enemyQuery = state.GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();

            float3 playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;

            float deltaTime = SystemAPI.Time.DeltaTime;

            int aliveEnemies = _enemyQuery.CalculateChunkCount();

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var enemySpawner in SystemAPI.Query<RefRW<EnemySpawner>>())
            {
                ref EnemySpawner spawner = ref enemySpawner.ValueRW;

                if (spawner.SpawnRate <= 0f)
                    continue;
                
                if (aliveEnemies >= spawner.MaxAlive)
                    continue;

                spawner.SpawnAccumulator += spawner.SpawnRate * deltaTime;

                int spawnCount = (int)math.floor(spawner.SpawnAccumulator);

                if (spawnCount <= 0)
                    continue;
                
                int availableSlots = spawner.MaxAlive - aliveEnemies;

                spawnCount = math.min(spawnCount, availableSlots);

                spawner.SpawnAccumulator -= spawnCount;

                uint seed = math.max(1u, spawner.RandomState);

                var random = new Random(seed);

                for (int i = 0; i < spawnCount; i++)
                {
                    float angle = random.NextFloat(0f, Mathf.PI * 2f);

                    float distance = random.NextFloat(spawner.MinSpawnRadius, spawner.MaxSpawnRadius);

                    float2 direction = new float2(math.cos(angle), math.sin(angle));
                    float2 spawnPosition = playerPosition.xy + direction * distance;
                    
                    Entity enemy = ecb.Instantiate(spawner.Prefab);
                    
                    ecb.SetComponent(enemy, LocalTransform.FromPosition(new float3(spawnPosition.x, spawnPosition.y, 0f)));
                }

                spawner.RandomState = random.state;

                aliveEnemies += spawnCount;
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}