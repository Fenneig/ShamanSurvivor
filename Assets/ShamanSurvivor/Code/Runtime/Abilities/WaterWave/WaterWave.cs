using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct WaterWave : IComponentData
    {
        public float HalfWidth;
        public float HalfDepth;
        public float KnockbackDistance;
        public float KnockbackSpeed;
    }
}