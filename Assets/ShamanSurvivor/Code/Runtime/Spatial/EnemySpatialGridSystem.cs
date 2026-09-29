using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameSpatialSystemGroup))]
    public partial struct EnemySpatialGridSystem : ISystem
    {
        public NativeParallelMultiHashMap<int2, EnemySpatialEntry> Grid;

        public JobHandle BuildHandle;

        private JobHandle _readerHandle;

        private EntityQuery _enemyQuery;

        private int _capacity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _capacity = 4096;
            
            Grid = new NativeParallelMultiHashMap<int2, EnemySpatialEntry>(_capacity, Allocator.Persistent);

            _enemyQuery = SystemAPI.QueryBuilder().WithAll<EnemyTag, LocalTransform, HitRadius, Health>().Build();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            JobHandle.CombineDependencies(BuildHandle, _readerHandle).Complete();

            if (Grid.IsCreated)
                Grid.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            int enemyCount = _enemyQuery.CalculateEntityCount();

            JobHandle dependency = JobHandle.CombineDependencies(state.Dependency, BuildHandle, _readerHandle);

            _readerHandle = default;

            if (_capacity < enemyCount)
            {
                dependency.Complete();

                _capacity = math.max(enemyCount, _capacity * 2);

                Grid.Capacity = _capacity;
            }
            
            JobHandle clearHandle = new ClearGridJob { Grid = Grid }.Schedule(state.Dependency);
            
            if (enemyCount == 0)
            {
                BuildHandle = clearHandle;

                state.Dependency = clearHandle;

                return;
            }

            var buildJob = new BuildGridJob { Grid = Grid.AsParallelWriter() };

            BuildHandle = buildJob.ScheduleParallel(_enemyQuery, clearHandle);

            state.Dependency = BuildHandle;
        }

        public void RegisterReader(JobHandle readerHandle)
        {
            _readerHandle = JobHandle.CombineDependencies(_readerHandle, readerHandle);
        }
        
        [BurstCompile]
        private partial struct ClearGridJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int2, EnemySpatialEntry> Grid;

            private void Execute()
            {
                Grid.Clear();
            }
        }

        [BurstCompile]
        private partial struct BuildGridJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ParallelWriter Grid;

            private void Execute(
                Entity entity,
                in LocalTransform transform,
                in HitRadius radius,
                in Health health)
            {
                if (health.Current <= 0f)
                    return;

                float2 position = transform.Position.xy;

                int2 cell = EnemySpatialGrid.PositionToCell(position);

                Grid.Add(cell, new EnemySpatialEntry
                { 
                    Entity = entity,
                    Position = position,
                    Radius = radius.Value
                });
            }
        }
    }
}