using ShamanSurvivor.Configs;
using ShamanSurvivor.Runtime;
using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Authoring
{
    public class LightningAbilityAuthoring : MonoBehaviour
    {
        [SerializeField] private LightningConfig _lightningConfig;
        
        public class LightningAbilityBaker : Baker<LightningAbilityAuthoring>
        {
            public override void Bake(LightningAbilityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new LightningAbility
                    {
                        ProjectilePrefab = GetEntity(authoring._lightningConfig.ProjectilePrefab, TransformUsageFlags.Dynamic),
                        Damage = authoring._lightningConfig.Damage,
                        Range = authoring._lightningConfig.Range,
                        AttackInterval = authoring._lightningConfig.AttackInterval,
                        CooldownRemaining = 0,
                        ProjectileSpeed = authoring._lightningConfig.ProjectileSpeed,
                        ProjectileLifetime = authoring._lightningConfig.ProjectileLifetime
                    });
            }
        }
    }
}