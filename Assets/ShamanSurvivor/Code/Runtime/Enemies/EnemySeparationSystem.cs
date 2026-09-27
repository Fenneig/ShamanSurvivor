using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameSeparationSystemGroup))]
    public partial struct EnemySeparationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnemySeparation>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            var grid = gridSystem.Grid.AsReadOnly();
            JobHandle dependency = JobHandle.CombineDependencies(state.Dependency, gridSystem.BuildHandle);
            var job = new SeparationJob { Grid = grid, DeltaTime = SystemAPI.Time.DeltaTime };
            JobHandle separationHandle = job.ScheduleParallel(dependency);
            state.Dependency = separationHandle;
            gridSystem.RegisterReader(separationHandle);
        }

        [BurstCompile]
        [WithAll(typeof(EnemyTag))]
        public partial struct SeparationJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ReadOnly Grid;
            public float DeltaTime;
            
            private void Execute(Entity entity, ref LocalTransform transform, in HitRadius hitRadius, in EnemySeparation separation)
            {
                if (separation.Strength <= 0f)
                    return;
                
                float2 position = transform.Position.xy;
                float2 correction = SpatialQuery.CalculateSeparation(Grid, entity, position, hitRadius.Value, separation.SearchRadius, separation.PersonalSpace);
                float correctionSq = math.lengthsq(correction);
                
                if (correctionSq <= 0.000001f)
                    return;

                float alpha = 1f - math.exp(-separation.Strength * DeltaTime);
                position += correction * alpha;
                float3 newPosition = transform.Position;
                newPosition.xy = position;
                transform.Position = newPosition;
            }
        }
    }
}