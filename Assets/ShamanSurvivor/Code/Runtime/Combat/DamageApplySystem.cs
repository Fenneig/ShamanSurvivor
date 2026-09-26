using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Code.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    [UpdateAfter(typeof(ContactDamageSystem))]
    public partial struct DamageApplySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Health>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (health, damageEvent) in SystemAPI.Query<RefRW<Health>, DynamicBuffer<DamageEvent>>())
            {
                if (damageEvent.Length == 0)
                    continue;

                float totalDamage = 0f;

                for (int i = 0; i < damageEvent.Length; i++)
                {
                    DamageEvent damage = damageEvent[i];
                    if (damage.Amount <= 0)
                        continue;

                    totalDamage += damage.Amount;
                }

                health.ValueRW.Current = math.max(0f, health.ValueRO.Current - totalDamage);
                
                damageEvent.Clear();
            }
        }
    }
}