using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Code.Runtime
{
    public class LightningAbilityAuthoring : MonoBehaviour
    {
        [Header("Projectile")] 
        [SerializeField] private GameObject _projectilePrefab;
        [Min(0f)] 
        [SerializeField] private float _projectileSpeed;
        [Min(0.1f)]
        [SerializeField] private float _projectileLifetime;

        [Header("Combat")] 
        [Min(0f)]
        [SerializeField] private float _damage;
        [Min(0f)] 
        [SerializeField] private float _attackInterval;
        [Min(0f)] 
        [SerializeField] private float _range;

        public class LightningAbilityBaker : Baker<LightningAbilityAuthoring>
        {
            public override void Bake(LightningAbilityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new LightningAbility
                    {
                        ProjectilePrefab = GetEntity(authoring._projectilePrefab, TransformUsageFlags.Dynamic),
                        Damage = authoring._damage,
                        Range = authoring._range,
                        AttackInterval = authoring._attackInterval,
                        CooldownRemaining = 0,
                        ProjectileSpeed = authoring._projectileSpeed,
                        ProjectileLifetime = authoring._projectileLifetime
                    });
            }
        }
    }
}