using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct ProjectileAbilityDefinition : IBufferElementData
    {
        public AbilityId Ability;
        public DamageElement DamageElement;
        
        public Entity ProjectilePrefab;

        public float Damage;
        public float Range;
        public float AttackInterval;
        
        public float ProjectileSpeed;
        public float ProjectileLifetime;
    }
}