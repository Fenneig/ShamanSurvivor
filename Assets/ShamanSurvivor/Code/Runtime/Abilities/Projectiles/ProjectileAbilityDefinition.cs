using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct ProjectileAbilityDefinition : IBufferElementData
    {
        public AbilityId Ability;
        
        public Entity ProjectilePrefab;
        
        public float ProjectileSpeed;
        public float ProjectileLifetime;
    }
}