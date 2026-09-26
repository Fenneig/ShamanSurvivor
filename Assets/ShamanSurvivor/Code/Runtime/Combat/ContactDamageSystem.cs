using ShamanSurvivor.Code.Runtime.Player;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    public partial struct ContactDamageSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<ContactDamage>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            
            LocalTransform playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
            
            HitRadius playerRadius = SystemAPI.GetComponent<HitRadius>(playerEntity);
            
            DynamicBuffer<DamageEvent> playerDamageBuffer = SystemAPI.GetBuffer<DamageEvent>(playerEntity);

            float2 playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position.xy;

            foreach (var (transform, 
                         enemyRadius,
                         contactDamage,
                         entity) in SystemAPI.Query<
                         RefRO<LocalTransform>,
                         RefRO<HitRadius>,
                         RefRW<ContactDamage>>()
                         .WithAll<EnemyTag>()
                         .WithEntityAccess())
            {
                ref ContactDamage damage = ref contactDamage.ValueRW;
                
                damage.CooldownRemaining -= deltaTime;
                if (damage.CooldownRemaining > 0)
                    continue;
                
                float2 enemyPosition = transform.ValueRO.Position.xy;

                float radius = playerRadius.Value + enemyRadius.ValueRO.Value;

                float distanceSq = math.distancesq(playerPosition, enemyPosition);

                if (distanceSq > radius * radius)
                    continue;

                playerDamageBuffer.Add(new DamageEvent
                {
                    Source = entity,
                    Amount = damage.Damage,
                    Element = DamageElement.Physical
                });
                
                damage.CooldownRemaining = damage.Interval;
            }
        }
    }
}