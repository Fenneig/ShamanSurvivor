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
            using NativeList<EnemySpatialEntry> targets = new NativeList<EnemySpatialEntry>(Allocator.Temp);
            
            ComponentLookup<Dead> deadLookup = SystemAPI.GetComponentLookup<Dead>();
            ComponentLookup<Health> healthLookup = SystemAPI.GetComponentLookup<Health>(true);
            BufferLookup<DamageEvent> damageLookup = SystemAPI.GetBufferLookup<DamageEvent>();
            
            foreach (var (projectile, hit, lavaImpact, snapshot, entity) in 
                     SystemAPI.Query<RefRO<Projectile>, RefRO<ProjectileHitEvent>, RefRO<LavaImpact>, RefRO<ProjectileModifierSnapshot>>().WithEntityAccess())
            {
                float effectiveSize = lavaImpact.ValueRO.ExplosionRadius * snapshot.ValueRO.SizeMultiplier;
                SpatialQuery.CollectInRadius(grid, hit.ValueRO.Position, effectiveSize, targets);

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

                    DamageUtility.TryAddDamage(target.Entity, ref healthLookup, ref damageLookup, ref deadLookup,
                        new DamageEvent
                        {
                            Amount = finalDamage,
                            Element = projectile.ValueRO.Element,
                            Source = projectile.ValueRO.Source
                        });
                }
                
                ecb.DestroyEntity(entity);
            }
        }
    }
}