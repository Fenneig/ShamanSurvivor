using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct LavaImpact : IComponentData
    {
        public float ExplosionRadius;

        public float BurnDamagePerTick;
        public float BurnTickInterval;
        public float BurnDuration;

        public float BurningTargetDamageMultiplier;
    }
}