using System;
using ShamanSurvivor.Shared;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameAbilitySystemGroup))]
    public partial struct LightningShieldOrbitSystem : ISystem
    {
        private const double TWO_PI = Math.PI * 2.0;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<AbilityCatalogTag>();
            state.RequireForUpdate<LightningShieldDefinition>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();

            EntityManager entityManager = state.EntityManager;

            Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();

            DynamicBuffer<AbilityState> abilities = entityManager.GetBuffer<AbilityState>(player);

            int abilityIndex = -1;

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i].Ability == AbilityId.LightningShield)
                {
                    abilityIndex = i;
                    break;
                }
            }

            if (abilityIndex < 0)
            {
                DeactivateAll(entityManager, player);

                var inactiveOrbit = entityManager.GetComponentData<LightningShieldOrbitState>(player);

                inactiveOrbit.Initialized = false;
                entityManager.SetComponentData(player, inactiveOrbit);

                return;
            }

            Entity catalog = SystemAPI.GetSingletonEntity<AbilityCatalogTag>();
            LightningShieldDefinition shield = entityManager.GetComponentData<LightningShieldDefinition>(catalog);
            DynamicBuffer<AbilityDefinition> definitions = entityManager.GetBuffer<AbilityDefinition>(catalog, true);
            AbilityDefinition definition = default;
            bool found = false;

            for (int i = 0; i < definitions.Length; i++)
            {
                if (definitions[i].Ability != AbilityId.LightningShield)
                    continue;

                definition = definitions[i];
                found = true;
                break;
            }

            if (!found)
            {
                DeactivateAll(entityManager, player);
                return;
            }

            DynamicBuffer<UpgradeProgress> upgrades = entityManager.GetBuffer<UpgradeProgress>(player, true);

            float damageBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.LightningShield, UpgradeKey.Damage);
            float frequencyBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.LightningShield, UpgradeKey.Frequency);
            float quantityBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.LightningShield, UpgradeKey.Quantity);
            float sizeBonus = UpgradeProgressUtility.GetBonus(upgrades, AbilityId.LightningShield, UpgradeKey.Size);
            int count = math.max(1, shield.BaseOrbCount + (int)math.round(quantityBonus));
            float sizeMultiplier = 1f + sizeBonus;
            float orbitRadius = shield.OrbitRadius * sizeMultiplier;
            float hitRadius = shield.OrbHitRadius * sizeMultiplier;
            float damage = definition.Damage * (1f + damageBonus);

            float period = math.max(0.01f, definition.AttackInterval / math.max(0.01f, 1f + frequencyBonus));
            double angularSpeed = TWO_PI / period;
            float deltaTime = SystemAPI.Time.DeltaTime;
            LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(player);
            float2 center = playerTransform.Position.xy;
            LightningShieldOrbitState orbit = entityManager.GetComponentData<LightningShieldOrbitState>(player);
            double previousAngle = orbit.TotalAngle;
            
            orbit.TotalAngle += angularSpeed * deltaTime;
            
            bool resetSweep = !orbit.Initialized || orbit.PreviousCount != count || math.abs(orbit.PreviousRadius - orbitRadius) > 0.001f;

            EnsurePoolCapacity(entityManager, player, count);

            DynamicBuffer<LightningOrbReference> pool = entityManager.GetBuffer<LightningOrbReference>(player);

            for (int i = 0; i < pool.Length; i++)
            {
                Entity orbEntity = pool[i].Value;

                bool shouldBeActive = i < count;

                bool currentlyActive = entityManager.IsComponentEnabled<LightningOrbActive>(orbEntity);

                if (currentlyActive != shouldBeActive)
                {
                    entityManager.SetComponentEnabled<LightningOrbActive>(orbEntity, shouldBeActive);

                    if (shouldBeActive) 
                        entityManager.GetBuffer<LightningOrbHit>(orbEntity).Clear();
                }

                if (!shouldBeActive)
                    continue;

                double startAngle = resetSweep ? orbit.TotalAngle : previousAngle;
                float2 startCenter = resetSweep ? center : orbit.PreviousPlayerPosition;

                LightningOrb orb = new LightningOrb
                {
                    Owner = player,
                    Slot = i,
                    OrbCount = count,

                    FromCenter = startCenter,
                    ToCenter = center,

                    FromAngle = startAngle,
                    ToAngle = orbit.TotalAngle,

                    OrbitRadius = orbitRadius,
                    HitRadius = hitRadius,

                    Damage = damage,
                    Element = definition.DamageElement
                };

                entityManager.SetComponentData(orbEntity, orb);
                float2 position = GetOrbitPosition(center, orbit.TotalAngle, i, count, orbitRadius);
                entityManager.SetComponentData(orbEntity, LocalTransform.FromPosition(new float3(position, 0f)));
            }

            orbit.PreviousPlayerPosition = center;
            orbit.PreviousCount = count;
            orbit.PreviousRadius = orbitRadius;
            orbit.Initialized = true;

            entityManager.SetComponentData(player, orbit);

            AbilityState ability = entityManager.GetBuffer<AbilityState>(player)[abilityIndex];
            
            ability.CooldownDuration = 1;
            ability.CooldownRemaining = 0;

            abilities[abilityIndex] = ability;
        }

        private float2 GetOrbitPosition(float2 center, double baseAngle, int index, int count, float radius)
        {
            double angle = baseAngle + TWO_PI * index / count;

            return center + new float2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
        }

        private void EnsurePoolCapacity(EntityManager entityManager, Entity player, int requiredCount)
        {
            int existingCount = entityManager.GetBuffer<LightningOrbReference>(player).Length;

            while (existingCount < requiredCount)
            {
                Entity orb = entityManager.CreateEntity(
                    typeof(LightningOrb),
                    typeof(LightningOrbActive),
                    typeof(LightningOrbHit),
                    typeof(LocalTransform));

                entityManager.SetComponentEnabled<LightningOrbActive>(orb, false);
                entityManager.SetComponentData(orb, new LightningOrb { Owner = player });
                entityManager.GetBuffer<LightningOrbReference>(player).Add(new LightningOrbReference { Value = orb });

                existingCount++;
            }
        }

        private void DeactivateAll(EntityManager entityManager, Entity player)
        {
            DynamicBuffer<LightningOrbReference> pool = entityManager.GetBuffer<LightningOrbReference>(player);

            for (int i = 0; i < pool.Length; i++)
            {
                Entity orb = pool[i].Value;

                if (entityManager.IsComponentEnabled<LightningOrbActive>(orb)) 
                    entityManager.SetComponentEnabled<LightningOrbActive>(orb, false);
            }
        }
    }
}