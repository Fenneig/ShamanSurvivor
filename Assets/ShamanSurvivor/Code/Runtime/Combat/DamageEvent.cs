using Unity.Entities;

namespace ShamanSurvivor.Code.Runtime
{
    [InternalBufferCapacity(4)]
    public struct DamageEvent : IBufferElementData
    {
        public Entity Source;
        public float Amount;
        public DamageElement Element;
    }
}