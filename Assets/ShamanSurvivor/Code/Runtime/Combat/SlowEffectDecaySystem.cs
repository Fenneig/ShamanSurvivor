using Unity.Burst;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameMovementSystemGroup))]
    [UpdateAfter(typeof(EarthquakeZoneSystem))]
    public partial struct SlowEffectDecaySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (DynamicBuffer<Slow> effects in SystemAPI.Query<DynamicBuffer<Slow>>())
            {
                DynamicBuffer<Slow> buffer = effects;
                for (int i = 0; i < buffer.Length; i++)
                {
                    Slow effect = buffer[i];

                    effect.RemainingDuration -=
                        deltaTime;

                    if (effect.RemainingDuration <= 0f)
                    {
                        buffer.RemoveAtSwapBack(i);
                        continue;
                    }

                    buffer[i] = effect;
                }
            }
        }
    }
}