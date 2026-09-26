using Unity.Entities;
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
                    new HomingProjectile
                    {
                        Source = Entity.Null,
                        Target = Entity.Null,
                        Speed = 0f,
                        Damage = 0f,
                        RemainingLifetime = 0f,
                        DamageElement = DamageElement.Physical
                    });
            }
        }
    }
}