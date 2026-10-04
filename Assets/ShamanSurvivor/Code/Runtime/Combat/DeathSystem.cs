using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
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

            bool hasDropSettings = SystemAPI.TryGetSingleton(out ExperienceDropSettings dropSettings);

            ComponentLookup<ExperienceReward> rewardLookup = SystemAPI.GetComponentLookup<ExperienceReward>(true);

            foreach (var (health, transform, entity) in SystemAPI.Query<RefRO<Health>, RefRO<LocalTransform>>()
                         .WithAll<DestroyOnDeath, Dead>()
                         .WithEntityAccess())
            {
                if (health.ValueRO.Current > 0f)
                    continue;

                if (hasDropSettings && rewardLookup.HasComponent(entity))
                {
                    ExperienceReward reward = rewardLookup[entity];
                    if (reward.Value > 0)
                    {
                        Entity orb = ecb.Instantiate(dropSettings.OrbPrefab);
                        LocalTransform orbTransform = SystemAPI.GetComponent<LocalTransform>(dropSettings.OrbPrefab);
                        orbTransform.Position = transform.ValueRO.Position;
                        orb.Set(ecb, orbTransform);
                        ExperienceOrb orbData = SystemAPI.GetComponent<ExperienceOrb>(dropSettings.OrbPrefab);
                        orbData.Value = reward.Value;
                        ecb.SetComponent(orb, orbData);
                    }
                }
                
                ecb.DestroyEntity(entity);
            }
        }
    }
}