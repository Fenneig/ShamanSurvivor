using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public sealed class AbilityCatalogAuthoring : MonoBehaviour
    {
        [SerializeField] private AbilityCatalogConfig _config;

        private class Baker : Baker<AbilityCatalogAuthoring>
        {
            public override void Bake(AbilityCatalogAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(entity, new AbilityCatalogTag());
                
                DynamicBuffer<AbilityDefinition> abilities = AddBuffer<AbilityDefinition>(entity);
                DynamicBuffer<ProjectileAbilityDefinition> projectiles = AddBuffer<ProjectileAbilityDefinition>(entity);
                
                foreach (var ability in authoring._config.Abilities)
                {
                    abilities.Add(new AbilityDefinition
                    {
                        Ability = ability.Ability,
                        DamageElement = ability.DamageElement,
                        AttackInterval = ability.AttackInterval,
                        Damage =  ability.Damage,
                        Range = ability.Range,
                        CanUnlock =  ability.CanUnlock,
                    });
                    
                    if (ability is ProjectileAbilityConfig projectileAbility)
                    {
                        Entity prefabEntity = GetEntity(projectileAbility.ProjectilePrefab, TransformUsageFlags.Dynamic);

                        projectiles.Add(new ProjectileAbilityDefinition
                        {
                            Ability = ability.Ability,
                            ProjectilePrefab = prefabEntity,
                            ProjectileLifetime = projectileAbility.ProjectileLifetime,
                            ProjectileSpeed = projectileAbility.ProjectileSpeed,
                        });
                        continue;
                    }

                    if (ability is ChainLightningConfig chain)
                    {
                        AddComponent(entity, new ChainLightningDefinition
                            {
                                Ability = chain.Ability,
                                JumpRange = chain.JumpRange,
                                BaseJumps = chain.BaseJumps
                            });
                    }
                    
                    if (ability is EarthquakeConfig earthquake)
                    {
                        AddComponent(entity, new EarthquakeDefinition
                        {
                            Ability = earthquake.Ability,
                            Radius = earthquake.Radius,
                            Duration = earthquake.Duration,
                            TickInterval = earthquake.TickInterval,
                            SlowAmount = earthquake.SlowAmount
                        });
                    }
                }
            }
        }
    }
}