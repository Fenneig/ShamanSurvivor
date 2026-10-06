using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace ShamanSurvivor.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameAbilitySystemGroup))]
    [UpdateAfter(typeof(ProjectileAbilitySystem))]
    public partial struct LinearProjectileMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<ProjectileTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            ComponentLookup<Dead> deadLookup = SystemAPI.GetComponentLookup<Dead>();
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            JobHandle dependency = JobHandle.CombineDependencies(state.Dependency, gridSystem.BuildHandle);

            var job = new MoveProjectileJob
            {
                Grid = gridSystem.Grid.AsReadOnly(),
                ECB = ecb,
                DeltaTime = SystemAPI.Time.DeltaTime,
                DeadLookup = deadLookup
            };

            JobHandle handle = job.Schedule(dependency);
            gridSystem.RegisterReader(handle);
            state.Dependency = handle;
        }
    }
}