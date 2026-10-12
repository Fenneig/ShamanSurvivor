using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct EarthWave : IComponentData
    {
        public float HalfWidth;
        public float HalfDepth;

        public float SlowAmount;
        public float SlowDuration;
    }
}