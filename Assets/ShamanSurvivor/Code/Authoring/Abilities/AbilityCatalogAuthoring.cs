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
                
                DynamicBuffer<ProjectileAbilityDefinition> definitions = AddBuffer<ProjectileAbilityDefinition>(entity);

                foreach (var ability in authoring._config.Abilities)
                {
                    Entity prefabEntity = GetEntity(ability.ProjectilePrefab, TransformUsageFlags.Dynamic);
                    definitions.Add(new ProjectileAbilityDefinition
                    {
                        Ability = ability.Ability,
                        DamageElement = ability.DamageElement,
                        AttackInterval = ability.AttackInterval,
                        Damage =  ability.Damage,
                        Range = ability.Range,
                        ProjectileLifetime =  ability.ProjectileLifetime,
                        ProjectilePrefab = prefabEntity,
                        ProjectileSpeed = ability.ProjectileSpeed
                    });
                }
            }
        }
    }
}