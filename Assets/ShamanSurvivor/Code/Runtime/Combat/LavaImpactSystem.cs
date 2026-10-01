using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    [UpdateBefore(typeof(DamageApplySystem))]
    public partial struct LavaImpactSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            gridSystem.BuildHandle.Complete();
            var grid = gridSystem.Grid.AsReadOnly();
            NativeList<EnemySpatialEntry> targets = new NativeList<EnemySpatialEntry>(Allocator.Temp);
            
            foreach (var (projectile, hit, lavaImpact, snapshot, entity) in 
                     SystemAPI.Query<RefRO<Projectile>, RefRO<ProjectileHitEvent>, RefRO<LavaImpact>, RefRO<ProjectileModifierSnapshot>>().WithEntityAccess())
            {
                targets.Clear();
                float effectiveSize = lavaImpact.ValueRO.ExplosionRadius * snapshot.ValueRO.SizeMultiplier;
                SpatialQuery.CollectInRadius(grid, hit.ValueRO.Position, effectiveSize, ref targets);

                foreach (var target in targets)
                {
                    if (!SystemAPI.HasBuffer<DamageEvent>(target.Entity))
                        continue;
                    
                    float finalDamage = projectile.ValueRO.Damage * snapshot.ValueRO.DamageMultiplier;

                    Burning burningComponent = SystemAPI.GetComponent<Burning>(target.Entity);
                    bool wasBurning = burningComponent.RemainingDuration > 0;

                    if (wasBurning)
                        finalDamage *= lavaImpact.ValueRO.BurningTargetDamageMultiplier;

                    float effectiveDuration = lavaImpact.ValueRO.BurnDuration * snapshot.ValueRO.DurationMultiplier;
                    float effectiveBurnDamage = lavaImpact.ValueRO.BurnDamagePerTick * snapshot.ValueRO.DamageMultiplier;

                    burningComponent = new Burning
                    {
                        Source = projectile.ValueRO.Source,
                        DamagePerTick = effectiveBurnDamage,
                        RemainingDuration = effectiveDuration,
                        TickInterval = lavaImpact.ValueRO.BurnTickInterval,
                        TickTimer = wasBurning ? burningComponent.TickTimer : lavaImpact.ValueRO.BurnTickInterval
                    };
                    
                    SystemAPI.SetComponent(target.Entity, burningComponent);
                    
                    DynamicBuffer<DamageEvent> damageBuffer = SystemAPI.GetBuffer<DamageEvent>(target.Entity);
                    damageBuffer.Add(new DamageEvent
                    {
                        Amount = finalDamage,
                        Element =  projectile.ValueRO.Element,
                        Source = projectile.ValueRO.Source
                    });
                }
                
                ecb.DestroyEntity(entity);
            }
        }
    }
}