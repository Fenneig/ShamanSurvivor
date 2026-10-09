using ShamanSurvivor.Shared;
using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    public struct EarthquakeZone : IComponentData
    {
        public Entity Source;

        public DamageElement DamageElement;

        public float2 Position;

        public float Radius;

        public float DamagePerTick;

        public float RemainingDuration;

        public float TickInterval;
        public float TickTimer;

        public float SlowAmount;
    }
}