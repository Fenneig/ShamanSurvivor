using ShamanSurvivor.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameAbilitySystemGroup))]
    public partial struct LightningAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<LightningAbility>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();
            
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            
            RefRW<LightningAbility> ability = SystemAPI.GetComponentRW<LightningAbility>(playerEntity);
            DynamicBuffer<UpgradeProgress> upgrades = SystemAPI.GetBuffer<UpgradeProgress>(playerEntity);
            float damageBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.Lightning, UpgradeKey.Damage);
            float frequencyBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.Lightning, UpgradeKey.Frequency);
            float quantityBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.Lightning, UpgradeKey.Quantity);
            
            float deltaTime = SystemAPI.Time.DeltaTime;
            ability.ValueRW.CooldownRemaining -= deltaTime;
            
            if (ability.ValueRO.CooldownRemaining > 0f)
                return;
            
            float3 playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;
            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            gridSystem.BuildHandle.Complete();
            var grid = gridSystem.Grid.AsReadOnly();
            bool targetFound = SpatialQuery.TryFindNearest(grid, playerPosition.xy, ability.ValueRO.Range, out EnemySpatialEntry target);

            if (!targetFound)
                return;

            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            LocalTransform projectileTransform = SystemAPI.GetComponent<LocalTransform>(ability.ValueRO.ProjectilePrefab);
            projectileTransform.Position = playerPosition;
            float2 direction = math.normalizesafe(target.Position - playerPosition.xy);
            
            float effectiveDamage = ability.ValueRO.Damage * (1 + damageBonus);
            float effectiveAttackInterval = ability.ValueRO.AttackInterval / (1f + frequencyBonus);
            int projectileCount = 1 + (int)quantityBonus;
            for (int i = 0; i < projectileCount; i++)
            {
                Entity projectile = ecb.Instantiate(ability.ValueRO.ProjectilePrefab);
                projectile.Set(ecb, projectileTransform);
            
                ecb.SetComponent(projectile, new Projectile
                {
                    Source = playerEntity,
                    Direction = direction,
                    Speed = ability.ValueRO.ProjectileSpeed,
                    Damage = effectiveDamage,
                    RemainingLifetime = ability.ValueRO.ProjectileLifetime,
                    Element = DamageElement.Lightning
                });
            }
            
            ability.ValueRW.CooldownRemaining = effectiveAttackInterval;
        }
    }
}