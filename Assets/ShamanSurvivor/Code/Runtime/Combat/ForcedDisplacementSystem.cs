using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    [UpdateAfter(typeof(WaterWaveSystem))]
    public partial struct ForcedDisplacementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (
                         transform,
                         displacement)
                     in SystemAPI.Query<
                         RefRW<LocalTransform>,
                         RefRW<ForcedDisplacement>>())
            {
                if (displacement.ValueRO.RemainingDistance <= 0f)
                    continue;

                float distance = math.min(displacement.ValueRO.Speed * deltaTime, displacement.ValueRO.RemainingDistance);

                float3 position = transform.ValueRO.Position;
                position.xy += displacement.ValueRO.Direction * distance;
                transform.ValueRW.Position = position;
                displacement.ValueRW.RemainingDistance -= distance;

                if (displacement.ValueRW.RemainingDistance <= 0f)
                {
                    displacement.ValueRW.RemainingDistance = 0f;
                    displacement.ValueRW.Speed = 0f;
                }
            }
        }
    }
}