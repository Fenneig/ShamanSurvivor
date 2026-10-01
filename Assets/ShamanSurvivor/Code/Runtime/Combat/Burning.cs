using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct Burning : IComponentData
    {
        public Entity Source;

        public float DamagePerTick;
        public float TickInterval;
        public float TickTimer;

        public float RemainingDuration;
    }
}