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
            ComponentLookup<Dead> deadLookup = SystemAPI.GetComponentLookup<Dead>();
            ComponentLookup<Health> healthLookup = SystemAPI.GetComponentLookup<Health>(true);
            BufferLookup<DamageEvent> damageLookup = SystemAPI.GetBufferLookup<DamageEvent>();

            foreach (var (projectile,
                         hit,
                         snapshot,
                         entity) in SystemAPI.Query<
                             RefRO<Projectile>,
                             RefRO<ProjectileHitEvent>,
                             RefRO<ProjectileModifierSnapshot>>()
                         .WithEntityAccess()
                         .WithAll<DirectImpact>())
            {
                Entity target = hit.ValueRO.Target;

                if (!SystemAPI.Exists(target) ||
                    !deadLookup.HasComponent(target) ||
                    deadLookup.IsComponentEnabled(target))
                {
                    ecb.RemoveComponent<ProjectileHitEvent>(entity);
                    continue;
                }

                float finalDamage = projectile.ValueRO.Damage * snapshot.ValueRO.DamageMultiplier;

                DamageUtility.TryAddDamage(target, ref healthLookup, ref damageLookup, ref deadLookup, 
                    new DamageEvent
                    {
                        Source = projectile.ValueRO.Source,
                        Amount = finalDamage,
                        Element = projectile.ValueRO.Element
                    });
                
                ecb.DestroyEntity(entity);
            }
        }
    }
}