using ShamanSurvivor.Shared;
using Unity.Burst;
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
            DynamicBuffer<ProjectileAbilityDefinition> definitions = SystemAPI.GetBuffer<ProjectileAbilityDefinition>(catalogEntity);

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

                ability.CooldownRemaining -= deltaTime;

                if (ability.CooldownRemaining > 0f)
                {
                    abilities[i] = ability;
                    continue;
                }
                if (!TryGetDefinition(definitions, ability.Ability, out ProjectileAbilityDefinition definition))
                {
                    abilities[i] = ability;
                    continue;
                }
                if (!SpatialQuery.TryFindNearest(grid, playerPosition.xy, definition.Range, out EnemySpatialEntry target))
                {
                    abilities[i] = ability;
                    continue;
                }

                Fire(player, playerPosition, definition, upgrades, target, ecb, ref state);

                float frequencyBonus = UpgradeProgressUtility.GetBonus(upgrades, ability.Ability, UpgradeKey.Frequency);
                ability.CooldownRemaining = definition.AttackInterval / (1f + frequencyBonus);
                abilities[i] = ability;
            }
        }

        private void Fire(Entity player, 
            float3 playerPosition, 
            in  ProjectileAbilityDefinition definition, 
            DynamicBuffer<UpgradeProgress> upgrades, 
            in  EnemySpatialEntry target, 
            EntityCommandBuffer ecb,
            ref SystemState state)
        {
            LocalTransform projectileTransform = SystemAPI.GetComponent<LocalTransform>(definition.ProjectilePrefab);
            float damageBonus = UpgradeProgressUtility.GetBonus(upgrades, definition.Ability, UpgradeKey.Damage);
            float quantityBonus = UpgradeProgressUtility.GetBonus(upgrades, definition.Ability, UpgradeKey.Quantity);
            float sizeBonus = UpgradeProgressUtility.GetBonus(upgrades, definition.Ability, UpgradeKey.Size);
            float durationBonus = UpgradeProgressUtility.GetBonus(upgrades, definition.Ability, UpgradeKey.Duration);
            float effectBonus = UpgradeProgressUtility.GetBonus(upgrades, definition.Ability, UpgradeKey.EffectStrength);

            projectileTransform.Position = playerPosition;
            float2 direction = math.normalizesafe(target.Position - playerPosition.xy);

            int projectileCount = 1 + (int)quantityBonus;
            for (int i = 0; i < projectileCount; i++)
            {
                Entity projectile = ecb.Instantiate(definition.ProjectilePrefab);
                projectile.Set(ecb, projectileTransform);

                ecb.SetComponent(projectile, new Projectile
                {
                    Source = player,
                    Direction = direction,
                    Speed = definition.ProjectileSpeed,
                    Damage = definition.Damage,
                    RemainingLifetime = definition.ProjectileLifetime,
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

        private bool TryGetDefinition(DynamicBuffer<ProjectileAbilityDefinition> definitions, AbilityId abilityAbility, out ProjectileAbilityDefinition projectileAbilityDefinition)
        {
            foreach (var abilityDefinition in definitions)
            {
                if (abilityDefinition.Ability == abilityAbility)
                {
                    projectileAbilityDefinition = abilityDefinition;
                    return true;
                }
            }
            
            projectileAbilityDefinition = default;
            return false;
        }
    }
}