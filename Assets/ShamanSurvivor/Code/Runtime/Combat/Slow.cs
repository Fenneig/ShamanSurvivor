using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct Slow : IBufferElementData
    {
        public Entity Source;
        public float Amount;
        public float RemainingDuration;
    }
}