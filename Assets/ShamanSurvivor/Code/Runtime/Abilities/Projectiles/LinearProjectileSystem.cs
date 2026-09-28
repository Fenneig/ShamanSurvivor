using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameAbilitySystemGroup))]
    [UpdateAfter(typeof(LightningAbilitySystem))]
    public partial struct LinearProjectileSystem : ISystem
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
            BufferLookup<DamageEvent> damageBufferLookup = SystemAPI.GetBufferLookup<DamageEvent>();
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            JobHandle dependency = JobHandle.CombineDependencies(state.Dependency, gridSystem.BuildHandle);

            var job = new MoveProjectileJob
            {
                    Grid = gridSystem.Grid.AsReadOnly(),
                    DamageBufferLookup = damageBufferLookup,
                    ECB = ecb,
                    DeltaTime = SystemAPI.Time.DeltaTime
                };

            JobHandle handle = job.Schedule(dependency);
            state.Dependency = handle;
            gridSystem.RegisterReader(handle);
        }
    }
}