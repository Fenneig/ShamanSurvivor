using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ShamanSurvivor.Code.Runtime
{
    public class ProjectileAuthoring : MonoBehaviour
    {
        [Min(0f)]
        [SerializeField] private float _hitRadius;
        
        public class Baker : Baker<ProjectileAuthoring>
        {
            public override void Bake(ProjectileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new ProjectileTag());
                AddComponent(entity, new ProjectileBody { HitRadius = authoring._hitRadius });
                
                AddComponent(entity,
                    new Projectile
                    {
                        Source = Entity.Null,
                        Direction = float2.zero,
                        Speed = 0f,
                        Damage = 0f,
                        RemainingLifetime = 0f,
                        Element = DamageElement.Physical
                    });
            }
        }
    }
}