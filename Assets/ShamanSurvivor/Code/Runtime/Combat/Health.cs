using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct Health : IComponentData
    {
        public float Current;
        public float Max;
    }
}