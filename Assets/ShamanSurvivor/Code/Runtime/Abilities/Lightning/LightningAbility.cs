using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct LightningAbility : IComponentData
    {
        public Entity ProjectilePrefab;

        public float Damage;
        public float Range;
        
        public float AttackInterval;
        public float CooldownRemaining;
        
        public float ProjectileSpeed;
        public float ProjectileLifetime;
    }
}