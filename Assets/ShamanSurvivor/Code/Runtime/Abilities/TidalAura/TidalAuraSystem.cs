using ShamanSurvivor.Shared;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    [UpdateBefore(typeof(DamageApplySystem))]
    public partial struct TidalAuraSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<AbilityCatalogTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();
            DynamicBuffer<AbilityState> abilities = SystemAPI.GetBuffer<AbilityState>(player);

            int abilityIndex = -1;

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].Ability == AbilityId.TidalAura)
                {
                    abilityIndex = i;
                    break;
                }
            }

            if (abilityIndex < 0)
                return;

            Entity catalog = SystemAPI.GetSingletonEntity<AbilityCatalogTag>();
            DynamicBuffer<AbilityDefinition> definitions = SystemAPI.GetBuffer<AbilityDefinition>(catalog);
            AbilityDefinition definition = default;
            bool found = false;

            for (int i = 0; i < definitions.Length; i++)
            {
                if (definitions[i].Ability != AbilityId.TidalAura)
                    continue;

                definition = definitions[i];
                found = true;
                break;
            }

            if (!found)
                return;

            DynamicBuffer<UpgradeProgress> upgrades = SystemAPI.GetBuffer<UpgradeProgress>(player);

            float damageBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.TidalAura, UpgradeKey.Damage);
            float frequencyBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.TidalAura, UpgradeKey.Frequency);
            float sizeBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.TidalAura, UpgradeKey.Size);
            float interval = definition.AttackInterval / (1f + frequencyBonus);

            AbilityState ability = abilities[abilityIndex];

            float timer = ability.CooldownRemaining - SystemAPI.Time.DeltaTime;

            int damageTicks = 0;

            while (timer <= 0f)
            {
                damageTicks++;
                timer += interval;
            }

            ability.CooldownRemaining = timer;
            ability.CooldownDuration = interval;

            abilities[abilityIndex] = ability;

            if (damageTicks == 0)
                return;

            float damage = definition.Damage * (1f + damageBonus) * damageTicks;
            float radius = definition.Range * (1f + sizeBonus);

            float2 position = SystemAPI.GetComponent<LocalTransform>(player).Position.xy;
            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);

            state.Dependency.Complete();
            gridSystem.BuildHandle.Complete();

            var grid = gridSystem.Grid.AsReadOnly();

            ComponentLookup<Health> healthLookup = SystemAPI.GetComponentLookup<Health>(true);
            BufferLookup<DamageEvent> damageLookup = SystemAPI.GetBufferLookup<DamageEvent>();
            ComponentLookup<Dead> deadLookup = SystemAPI.GetComponentLookup<Dead>();
            using NativeList<EnemySpatialEntry> targets = new(Allocator.Temp);
            SpatialQuery.CollectInRadius(grid, position, radius, targets);

            for (int i = 0; i < targets.Length; i++)
            {
                Entity target = targets[i].Entity;

                DamageUtility.TryAddDamage(target, ref healthLookup, ref damageLookup, ref deadLookup,
                    new DamageEvent
                    {
                        Source = player,
                        Amount = damage,
                        Element = definition.DamageElement
                    });
            }
        }
    }
}