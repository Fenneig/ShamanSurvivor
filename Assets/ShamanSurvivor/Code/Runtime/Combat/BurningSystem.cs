using ShamanSurvivor.Shared;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    [UpdateBefore(typeof(DamageApplySystem))]
    public partial struct BurningSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            
            foreach (var (damageEvent, burning) in SystemAPI.Query<DynamicBuffer<DamageEvent>, RefRW<Burning>>())
            {
                if (burning.ValueRO.RemainingDuration <= 0)
                    continue;
                
                burning.ValueRW.RemainingDuration -= deltaTime;
                burning.ValueRW.TickTimer -= deltaTime;

                if (burning.ValueRO.TickTimer > 0)
                    continue;

                burning.ValueRW.TickTimer = burning.ValueRO.TickInterval;
                damageEvent.Add(new DamageEvent
                {
                    Source = burning.ValueRO.Source,
                    Amount = burning.ValueRO.DamagePerTick,
                    Element = DamageElement.Fire
                });
            }
        }
    }
}