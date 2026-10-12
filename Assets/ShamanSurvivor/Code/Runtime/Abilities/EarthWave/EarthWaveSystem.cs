using Unity.Burst;
using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    [UpdateBefore(typeof(DamageApplySystem))]
    [UpdateBefore(typeof(SlowEffectDecaySystem))]
    public partial struct EarthWaveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem =
                ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            JobHandle.CombineDependencies(state.Dependency, gridSystem.BuildHandle).Complete();
            var grid = gridSystem.Grid.AsReadOnly();

            ComponentLookup<Health> healthLookup = SystemAPI.GetComponentLookup<Health>(true);
            BufferLookup<DamageEvent> damageLookup = SystemAPI.GetBufferLookup<DamageEvent>();
            ComponentLookup<Dead> deadLookup = SystemAPI.GetComponentLookup<Dead>();
            BufferLookup<Slow> slowLookup = SystemAPI.GetBufferLookup<Slow>();
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            float deltaTime = SystemAPI.Time.DeltaTime;

            using NativeList<EnemySpatialEntry> targets = new(Allocator.Temp);

            foreach (var (transform,
                         projectile,
                         snapshot,
                         wave,
                         entity)
                     in SystemAPI.Query<
                             RefRW<LocalTransform>,
                             RefRW<Projectile>,
                             RefRO<ProjectileModifierSnapshot>,
                             RefRO<EarthWave>>()
                         .WithEntityAccess())
            {
                projectile.ValueRW.RemainingLifetime -= deltaTime;

                if (projectile.ValueRO.RemainingLifetime <= 0f)
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }

                float2 direction = math.normalizesafe(projectile.ValueRO.Direction, new float2(1f, 0f));
                float2 start = transform.ValueRO.Position.xy;
                float2 end = start + direction * projectile.ValueRO.Speed * deltaTime;
                float halfWidth = wave.ValueRO.HalfWidth * snapshot.ValueRO.SizeMultiplier;
                float halfDepth = wave.ValueRO.HalfDepth * snapshot.ValueRO.SizeMultiplier;

                SpatialQuery.CollectSweptEllipseHits(grid, start, end, direction, halfWidth, halfDepth, targets);
                DynamicBuffer<ProjectileHitHistory> history = SystemAPI.GetBuffer<ProjectileHitHistory>(entity);

                for (int i = 0; i < targets.Length; i++)
                {
                    Entity target = targets[i].Entity;

                    if (!deadLookup.HasComponent(target) || deadLookup.IsComponentEnabled(target))
                        continue;

                    if (DamageUtility.WasAlreadyHit(history, target))
                        continue;

                    if (!damageLookup.HasBuffer(target))
                        continue;

                    history.Add(new ProjectileHitHistory { Entity = target });

                    float damage = projectile.ValueRO.Damage * snapshot.ValueRO.DamageMultiplier;

                    DamageUtility.TryAddDamage(target, ref healthLookup, ref damageLookup, ref deadLookup, new DamageEvent
                        {
                            Source = projectile.ValueRO.Source,
                            Amount = damage,
                            Element = projectile.ValueRO.Element
                        });
                    
                    if (deadLookup.IsComponentEnabled(target))
                        continue;

                    if (!slowLookup.HasBuffer(target))
                        continue;

                    float slowAmount = wave.ValueRO.SlowAmount * snapshot.ValueRO.EffectStrengthMultiplier;
                    float slowDuration = wave.ValueRO.SlowDuration * snapshot.ValueRO.DurationMultiplier;
                    DynamicBuffer<Slow> slowEffects = slowLookup[target];
                    SlowUtility.ApplyOrRefresh(slowEffects, entity, math.clamp(slowAmount, 0f, SlowUtility.UPPER_BOUND), slowDuration);
                }

                float3 newPosition = transform.ValueRO.Position;
                newPosition.xy = end;
                transform.ValueRW.Position = newPosition;
            }
        }
    }
}