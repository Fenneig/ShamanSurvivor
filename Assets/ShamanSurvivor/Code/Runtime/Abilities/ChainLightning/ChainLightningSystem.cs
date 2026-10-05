using ShamanSurvivor.Shared;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShamanSurvivor.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameAbilitySystemGroup))]
    public partial struct ChainLightningSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<ChainLightningDefinition>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();

            DynamicBuffer<AbilityState> unlockedAbilities = SystemAPI.GetBuffer<AbilityState>(player);

            int abilityIndex = FindAbility(unlockedAbilities, AbilityId.ChainLightning);

            if (abilityIndex < 0)
                return;

            AbilityState ability = unlockedAbilities[abilityIndex];
            float deltaTime = SystemAPI.Time.DeltaTime;
            ability.CooldownRemaining -= deltaTime;

            if (ability.CooldownRemaining > 0f)
            {
                unlockedAbilities[abilityIndex] = ability;

                return;
            }

            DynamicBuffer<AbilityDefinition> abilities = SystemAPI.GetBuffer<AbilityDefinition>(player);
            
            AbilityDefinition definition = FindAbility(abilities, AbilityId.ChainLightning);
            ChainLightningDefinition chainLightningDefinition = SystemAPI.GetComponent<ChainLightningDefinition>(player);

            DynamicBuffer<UpgradeProgress> upgrades = SystemAPI.GetBuffer<UpgradeProgress>(player);

            float damageBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.ChainLightning, UpgradeKey.Damage);
            float frequencyBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.ChainLightning, UpgradeKey.Frequency);
            float quantityBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.ChainLightning, UpgradeKey.Quantity);
            
            float finalDamage = definition.Damage * (1f + damageBonus);
            int jumpCount = chainLightningDefinition.BaseJumps + (int)quantityBonus;
            
            LocalTransform playerTransform = SystemAPI.GetComponent<LocalTransform>(player);
            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            gridSystem.BuildHandle.Complete();
            var grid = gridSystem.Grid.AsReadOnly();

            ComponentLookup<Health> healthLookup = SystemAPI.GetComponentLookup<Health>(true);
            ComponentLookup<Dead> deadLookup = SystemAPI.GetComponentLookup<Dead>();
            BufferLookup<DamageEvent> damageLookup = SystemAPI.GetBufferLookup<DamageEvent>();
            FixedList128Bytes<Entity> hitEntities = default;
            float2 currentOrigin = playerTransform.Position.xy;

            if (!CombatTargetQuery.TryFindNearestDamageable(grid, currentOrigin, definition.Range, healthLookup, damageLookup, hitEntities, out EnemySpatialEntry currentTarget))
            {
                ability.CooldownRemaining = 0f;
                unlockedAbilities[abilityIndex] = ability;

                return;
            }

            int maxHits = 1 + jumpCount;

            for (int hitIndex = 0; hitIndex < maxHits; hitIndex++)
            {
                Entity target = currentTarget.Entity;

                if (!damageLookup.HasBuffer(target))
                    break;
                
                DamageUtility.TryAddDamage(target, ref healthLookup, ref damageLookup, ref deadLookup,
                    new DamageEvent
                    {
                        Source = player,
                        Amount = finalDamage,
                        Element = definition.DamageElement
                    });

                hitEntities.Add(target);
                currentOrigin = currentTarget.Position;

                if (hitIndex + 1 >= maxHits)
                    break;

                if (!CombatTargetQuery.TryFindNearestDamageable(grid, currentOrigin, chainLightningDefinition.JumpRange, healthLookup, damageLookup, hitEntities, out currentTarget))
                    break;
            }
            
            float cooldownDuration = definition.AttackInterval / (1f + frequencyBonus);
            ability.CooldownRemaining = cooldownDuration;
            ability.CooldownDuration = cooldownDuration;
            unlockedAbilities[abilityIndex] = ability;
        }

        private int FindAbility(DynamicBuffer<AbilityState> abilities, AbilityId ability)
        {
            for (int i = 0; i < abilities.Length; i++)
                if (abilities[i].Ability == ability)
                    return i;

            return -1;
        }
        
        private AbilityDefinition FindAbility(DynamicBuffer<AbilityDefinition> abilities, AbilityId ability)
        {
            for (int i = 0; i < abilities.Length; i++)
                if (abilities[i].Ability == ability)
                    return abilities[i];

            return default;
        }
    }
}