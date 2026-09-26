using Unity.Entities;

namespace ShamanSurvivor.Code.Runtime
{
    public struct Health : IComponentData
    {
        public float Current;
        public float Max;
    }
}