using Unity.Entities;
using UnityEngine;

namespace ShamanSurvivor.Code.Runtime
{
    public class EnemyAuthoring : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private float _maxHealth = 10f;
        
        [Header("Movement")]
        [SerializeField] private float _speed = 2f;
        
        [Header("Body")] 
        [SerializeField] private float _hitRadius = .35f;

        [Header("Contact Damage")] 
        [Min(0f)] 
        [SerializeField] private float _contactDamage = 10f;
        [Min(0.01f)]
        [SerializeField] private float _contactDamageInterval = 1f;
        
        [Header("Separation")]
        [Min(0.01f)] 
        [SerializeField] private float _separationSearchRadius = 1.25f;
        [Min(0f)]
        [SerializeField] private float _personalSpace = 0.05f;
        [Min(0f)]
        [SerializeField] private float _separationStrength = 1f;
        

        public class Baker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyTag());
                AddComponent(entity, new EnemyMovement { Speed = authoring._speed });
                AddComponent(entity, new Health { Max = authoring._maxHealth, Current = authoring._maxHealth });
                AddComponent(entity, new HitRadius { Value = authoring._hitRadius });
                AddComponent(entity, new ContactDamage{ Damage = authoring._contactDamage, Interval = authoring._contactDamageInterval });
                AddComponent(entity, new DestroyOnDeath());
                AddBuffer<DamageEvent>(entity);
                AddComponent(entity, new EnemySeparation
                {
                    SearchRadius = authoring._separationSearchRadius,
                    PersonalSpace = authoring._personalSpace,
                    Strength = authoring._separationStrength
                });
            }
        }
    }
}