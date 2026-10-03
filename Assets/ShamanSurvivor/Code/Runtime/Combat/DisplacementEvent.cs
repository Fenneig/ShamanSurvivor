using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    public struct ForcedDisplacement : IComponentData
    {
        public float2 Direction;
        public float RemainingDistance;
        public float Speed;
    }
}