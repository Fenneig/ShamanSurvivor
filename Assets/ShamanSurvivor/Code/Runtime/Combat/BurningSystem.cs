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
            state.Dependency.Complete();
            float deltaTime = SystemAPI.Time.DeltaTime;
            
            var healthLookup = SystemAPI.GetComponentLookup<Health>();
            var damageLookup = SystemAPI.GetBufferLookup<DamageEvent>();
            var deadLookup = SystemAPI.GetComponentLookup<Dead>();
            
            foreach (var (burning, entity) in SystemAPI.Query<RefRW<Burning>>().WithEntityAccess())
            {
                if (burning.ValueRO.RemainingDuration <= 0)
                    continue;
                
                burning.ValueRW.RemainingDuration -= deltaTime;
                burning.ValueRW.TickTimer -= deltaTime;

                if (burning.ValueRO.TickTimer > 0)
                    continue;

                burning.ValueRW.TickTimer = burning.ValueRO.TickInterval;
                                
                DamageUtility.TryAddDamage(entity, ref healthLookup, ref damageLookup, ref deadLookup,
                    new DamageEvent
                    {
                        Source = burning.ValueRO.Source,
                        Amount = burning.ValueRO.DamagePerTick,
                        Element = DamageElement.Fire
                    });
            }
        }
    }
}