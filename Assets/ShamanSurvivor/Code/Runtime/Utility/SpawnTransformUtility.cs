using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime
{
    public static class SpawnTransformUtility
    {
        public static void Set(this Entity entity, EntityCommandBuffer ecb, in LocalTransform transform)
        {
            ecb.SetComponent(entity, transform);

            ecb.SetComponent(entity, new LocalToWorld
            {
                Value = float4x4.TRS(
                    transform.Position,
                    transform.Rotation,
                    new float3(transform.Scale))
            });
        }
    }
}