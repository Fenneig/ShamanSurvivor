using ShamanSurvivor.Code.Runtime.Player;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime
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

            float deltaTime = SystemAPI.Time.DeltaTime;

            ability.ValueRW.CooldownRemaining -= deltaTime;
            
            if (ability.ValueRO.CooldownRemaining > 0f)
                return;
            
            float3 playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;

            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();

            ref EnemySpatialGridSystem gridSystem =
                ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            
            gridSystem.BuildHandle.Complete();

            var grid = gridSystem.Grid.AsReadOnly();

            bool isTargetValid = SpatialQuery.TryFindNearest(grid, playerPosition.xy, ability.ValueRO.Range, out EnemySpatialEntry target);

            if (!isTargetValid)
                return;

            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            LocalTransform projectileTransform = SystemAPI.GetComponent<LocalTransform>(ability.ValueRO.ProjectilePrefab);
            
            projectileTransform.Position = playerPosition;
            
            Entity projectile = ecb.Instantiate(ability.ValueRO.ProjectilePrefab);

            projectile.Set(ecb, projectileTransform);
            
            ecb.SetComponent(projectile, new HomingProjectile
            {
                Source = playerEntity,
                Target = target.Entity,
                Speed = ability.ValueRO.ProjectileSpeed,
                Damage = ability.ValueRO.Damage,
                RemainingLifetime = ability.ValueRO.ProjectileLifetime,
                DamageElement = DamageElement.Lightning
            });
            
            ability.ValueRW.CooldownRemaining = ability.ValueRO.AttackInterval;
        }
    }
}