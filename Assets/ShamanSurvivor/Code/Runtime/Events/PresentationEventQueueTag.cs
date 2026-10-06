using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    public struct PresentationEventQueueTag : IComponentData
    {
    }

    [InternalBufferCapacity(16)]
    public struct ChainLightningVisualEvent : IBufferElementData
    {
        public float2 From;
        public float2 To;

        public int StepIndex;
    }
}