using ShamanSurvivor.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameAbilitySystemGroup))]
    public partial struct ProjectileAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AbilityCatalogTag>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<ProjectileAbilityDefinition>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();

            float deltaTime = SystemAPI.Time.DeltaTime;

            DynamicBuffer<AbilityState> abilities = SystemAPI.GetBuffer<AbilityState>(player);
            DynamicBuffer<UpgradeProgress> upgrades = SystemAPI.GetBuffer<UpgradeProgress>(player);
            Entity catalogEntity = SystemAPI.GetSingletonEntity<AbilityCatalogTag>();
            DynamicBuffer<AbilityDefinition> definitions = SystemAPI.GetBuffer<AbilityDefinition>(catalogEntity);
            DynamicBuffer<ProjectileAbilityDefinition> projectileDefinitions = SystemAPI.GetBuffer<ProjectileAbilityDefinition>(catalogEntity);

            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            gridSystem.BuildHandle.Complete();
            var grid = gridSystem.Grid.AsReadOnly();
            float3 playerPosition = SystemAPI.GetComponent<LocalTransform>(player).Position;

            for (int i = 0; i < abilities.Length; i++)
            {
                AbilityState ability = abilities[i];
                
                if (!TryGetProjectileDefinition(projectileDefinitions, ability.Ability, out ProjectileAbilityDefinition projectileDefinition))
                    continue;
                if (!TryGetDefinition(definitions, ability.Ability, out AbilityDefinition definition))
                    continue;
                
                ability.CooldownRemaining -= deltaTime;

                if (ability.CooldownRemaining > 0f)
                {
                    abilities[i] = ability;
                    continue;
                }

                float quantityBonus = UpgradeProgressUtility.GetBonus(upgrades, definition.Ability, UpgradeKey.Quantity);
                int projectileCount = 1 + (int)quantityBonus;
                using NativeList<EnemySpatialEntry> targets = new NativeList<EnemySpatialEntry>(projectileCount, Allocator.Temp);
                SpatialQuery.CollectNearest(grid, playerPosition.xy, definition.Range, projectileCount, targets);
                
                if (targets.Count == 0)
                {
                    abilities[i] = ability;
                    continue;
                }

                Fire(player, playerPosition, definition, projectileDefinition, upgrades, ecb, targets, projectileCount, ref state);

                float frequencyBonus = UpgradeProgressUtility.GetBonus(upgrades, ability.Ability, UpgradeKey.Frequency);
                ability.CooldownRemaining = definition.AttackInterval / (1f + frequencyBonus);
                abilities[i] = ability;
            }
        }

        private void Fire(Entity player, 
            float3 playerPosition, 
            in AbilityDefinition definition,
            in  ProjectileAbilityDefinition projectileDefinition, 
            DynamicBuffer<UpgradeProgress> upgrades,
            EntityCommandBuffer ecb,
            NativeList<EnemySpatialEntry> targets,
            int projectileCount,
            ref SystemState state)
        {
            LocalTransform projectileTransform = SystemAPI.GetComponent<LocalTransform>(projectileDefinition.ProjectilePrefab);
            float damageBonus = UpgradeProgressUtility.GetBonus(upgrades, definition.Ability, UpgradeKey.Damage);
            float sizeBonus = UpgradeProgressUtility.GetBonus(upgrades, definition.Ability, UpgradeKey.Size);
            float durationBonus = UpgradeProgressUtility.GetBonus(upgrades, definition.Ability, UpgradeKey.Duration);
            float effectBonus = UpgradeProgressUtility.GetBonus(upgrades, definition.Ability, UpgradeKey.EffectStrength);
            
            projectileTransform.Position = playerPosition;

            for (int i = 0; i < projectileCount; i++)
            {
                Entity projectile = ecb.Instantiate(projectileDefinition.ProjectilePrefab);
                quaternion prefabRotation = SystemAPI.GetComponent<LocalTransform>(projectileDefinition.ProjectilePrefab).Rotation;
                float2 direction = math.normalizesafe(targets[i % targets.Length].Position - playerPosition.xy);
                float angle = math.atan2(direction.y, direction.x);
                quaternion directionRotation = quaternion.RotateZ(angle);
                projectileTransform.Rotation = math.mul(directionRotation, prefabRotation);
                
                projectile.Set(ecb, projectileTransform);
                
                ecb.SetComponent(projectile, new Projectile
                {
                    Source = player,
                    Direction = direction,
                    Speed = projectileDefinition.ProjectileSpeed,
                    Damage = definition.Damage,
                    RemainingLifetime = projectileDefinition.ProjectileLifetime,
                    Element = definition.DamageElement
                });

                ecb.SetComponent(projectile, new ProjectileModifierSnapshot
                {
                    SizeMultiplier = 1f + sizeBonus,
                    DurationMultiplier = 1f + durationBonus,
                    EffectStrengthMultiplier = 1f + effectBonus,
                    DamageMultiplier = 1f + damageBonus
                });
            }
        }

        private bool TryGetDefinition(DynamicBuffer<AbilityDefinition> definitions, AbilityId ability, out AbilityDefinition abilityDefinition)
        {
            foreach (var definition in definitions)
            {
                if (definition.Ability == ability)
                {
                    abilityDefinition = definition;
                    return true;
                }
            }
            
            abilityDefinition = default;
            return false;
        }
        

        private bool TryGetProjectileDefinition(DynamicBuffer<ProjectileAbilityDefinition> definitions, AbilityId ability, out ProjectileAbilityDefinition abilityDefinition)
        {
            foreach (var definition in definitions)
            {
                if (definition.Ability == ability)
                {
                    abilityDefinition = definition;
                    return true;
                }
            }
            
            abilityDefinition = default;
            return false;
        }
    }
}