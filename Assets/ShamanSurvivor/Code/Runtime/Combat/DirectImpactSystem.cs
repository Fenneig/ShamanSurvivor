using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    public partial struct DirectImpactSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (projectile, hit, entity) in SystemAPI.Query<RefRO<Projectile>, RefRO<ProjectileHitEvent>>().WithEntityAccess().WithAll<DirectImpact>())
            {
                Entity target = hit.ValueRO.Target;

                if (!SystemAPI.HasBuffer<DamageEvent>(target))
                    continue;
                
                DynamicBuffer<DamageEvent> damageBuffer = SystemAPI.GetBuffer<DamageEvent>(target);
                damageBuffer.Add(new DamageEvent
                {
                    Amount = projectile.ValueRO.Damage,
                    Element =  projectile.ValueRO.Element,
                    Source = projectile.ValueRO.Source
                });
                
                ecb.DestroyEntity(entity);
            }
        }
    }
}