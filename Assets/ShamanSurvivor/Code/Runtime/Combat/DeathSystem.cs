using Unity.Burst;
using Unity.Entities;

namespace ShamanSurvivor.Code.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    [UpdateAfter(typeof(DamageApplySystem))]
    public partial struct DeathSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<DestroyOnDeath>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (health, entity) in SystemAPI.Query<RefRO<Health>>()
                         .WithAll<DestroyOnDeath>()
                         .WithEntityAccess())
            {
                if (health.ValueRO.Current > 0f)
                    continue;
                
                ecb.DestroyEntity(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}