using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct LavaEruption : IComponentData
    {
        public Entity ProjectilePrefab;

        public float Range;

        public float Damage;
        public float ExplosionRadius;

        public float BurnDamagePerTick;
        public float BurnTickInterval;
        public float BurnDuration;

        public float AttackInterval;
        public float CooldownRemaining;

        public float ProjectileSpeed;
        public float ProjectileLifetime;
    }
}