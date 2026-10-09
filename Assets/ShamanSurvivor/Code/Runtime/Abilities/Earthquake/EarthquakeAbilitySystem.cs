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
    public partial struct EarthquakeAbilitySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<AbilityCatalogTag>();
            state.RequireForUpdate<EarthquakeDefinition>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();
            
            DynamicBuffer<AbilityState> abilityStates = SystemAPI.GetBuffer<AbilityState>(player);

            int stateIndex = SearchAbilityUtility.FindAbilityIndex(abilityStates, AbilityId.Earthquake);
            
            if (stateIndex < 0)
                return;
            
            AbilityState abilityState = abilityStates[stateIndex];
            
            abilityState.CooldownRemaining = math.max(0, abilityState.CooldownRemaining - SystemAPI.Time.DeltaTime);

            if (abilityState.CooldownRemaining > 0)
            {
                abilityStates[stateIndex] = abilityState;
                
                return;
            }

            Entity catalog = SystemAPI.GetSingletonEntity<AbilityCatalogTag>();
            DynamicBuffer<AbilityDefinition> definitions = SystemAPI.GetBuffer<AbilityDefinition>(catalog);

            if (!SearchAbilityUtility.TryGetDefinition(definitions, AbilityId.Earthquake, out AbilityDefinition definition))
            {
                abilityStates[stateIndex] = abilityState;
                
                return;
            }
            
            EarthquakeDefinition earthquake = SystemAPI.GetComponent<EarthquakeDefinition>(catalog);

            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            
            gridSystem.BuildHandle.Complete();

            var gird = gridSystem.Grid.AsReadOnly();

            ComponentLookup<Dead> deadLookup = SystemAPI.GetComponentLookup<Dead>(true);
            LocalTransform playerTransform = SystemAPI.GetComponent<LocalTransform>(player);
            FixedList128Bytes<Entity> excluded = default;

            if (!CombatTargetQuery.TryFindNearestDamageable(gird, playerTransform.Position.xy, definition.Range,
                    deadLookup, excluded, out EnemySpatialEntry target))
            {
                abilityState.CooldownRemaining = 0;
                abilityStates[stateIndex] = abilityState;
                return;
            }

            float2 zonePosition = SystemAPI.GetComponent<LocalTransform>(target.Entity).Position.xy;
            
            DynamicBuffer<UpgradeProgress> upgrades = SystemAPI.GetBuffer<UpgradeProgress>(player);

            float damageBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.Earthquake, UpgradeKey.Damage);
            float sizeBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.Earthquake, UpgradeKey.Size);
            float frequencyBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.Earthquake, UpgradeKey.Frequency);
            float durationBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.Earthquake, UpgradeKey.Duration);
            float effectBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.Earthquake, UpgradeKey.EffectStrength);

            float effectiveDamage = definition.Damage * (1 + damageBonus);
            float effectiveSize = earthquake.Radius * (1 + sizeBonus);
            float effectiveDuration = earthquake.Duration * (1 + durationBonus);
            float effectiveSlow = math.clamp(earthquake.SlowAmount * (1 + effectBonus), 0, 1f);
            
            float cooldown = definition.AttackInterval / (1 + frequencyBonus);
            
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            Entity zone = ecb.CreateEntity();
            ecb.AddComponent(zone, new EarthquakeZone
            {
                Source = player,
                DamageElement = DamageElement.Earth,
                Position = zonePosition,
                Radius = effectiveSize,
                DamagePerTick = effectiveDamage,
                RemainingDuration = effectiveDuration,
                TickInterval = earthquake.TickInterval,
                TickTimer = 0f,
                SlowAmount = effectiveSlow
            });

            abilityState.CooldownRemaining = cooldown;
            abilityState.CooldownDuration = cooldown;
            abilityStates[stateIndex] = abilityState;
        }
    }
}