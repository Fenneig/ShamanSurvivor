using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct EarthquakeDefinition : IComponentData
    {
        public AbilityId Ability;

        public float Radius;
        public float Duration;

        public float TickInterval;

        public float SlowAmount;
    }
}