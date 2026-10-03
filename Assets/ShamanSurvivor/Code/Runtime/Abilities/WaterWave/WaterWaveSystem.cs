using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    [UpdateBefore(typeof(DamageApplySystem))]
    public partial struct WaterWaveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem =
                ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);

            gridSystem.BuildHandle.Complete();

            var grid = gridSystem.Grid.AsReadOnly();
            float deltaTime = SystemAPI.Time.DeltaTime;

            using NativeList<EnemySpatialEntry> targets = new(Allocator.Temp);

            foreach (var (transform,
                         projectile,
                         wave,
                         snapshot,
                         entity) in SystemAPI.Query<
                             RefRW<LocalTransform>,
                             RefRW<Projectile>,
                             RefRO<WaterWave>,
                             RefRO<ProjectileModifierSnapshot>>()
                         .WithAll<PiercingProjectile>()
                         .WithEntityAccess())
            {
                projectile.ValueRW.RemainingLifetime -= deltaTime;

                if (projectile.ValueRO.RemainingLifetime <= 0f)
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }
                
                transform.ValueRW.Scale = snapshot.ValueRO.SizeMultiplier;
                float2 start = transform.ValueRO.Position.xy;
                float2 end = start + projectile.ValueRO.Direction * projectile.ValueRO.Speed * deltaTime;
                float halfWidth = wave.ValueRO.HalfWidth * snapshot.ValueRO.SizeMultiplier;
                float halfDepth = wave.ValueRO.HalfDepth * snapshot.ValueRO.SizeMultiplier;

                SpatialQuery.CollectSweptEllipseHits(grid, start, end, projectile.ValueRO.Direction, halfWidth, halfDepth, targets);
                DynamicBuffer<ProjectileHitHistory> history = SystemAPI.GetBuffer<ProjectileHitHistory>(entity);

                for (int i = 0; i < targets.Length; i++)
                {
                    EnemySpatialEntry target = targets[i];

                    if (WasAlreadyHit(history, target.Entity))
                        continue;

                    if (!SystemAPI.HasBuffer<DamageEvent>(target.Entity))
                        continue;

                    history.Add(new ProjectileHitHistory
                    {
                        Entity = target.Entity
                    });

                    float damage = projectile.ValueRO.Damage * snapshot.ValueRO.DamageMultiplier;

                    DynamicBuffer<DamageEvent> damageBuffer = SystemAPI.GetBuffer<DamageEvent>(target.Entity);

                    damageBuffer.Add(new DamageEvent
                    {
                        Source = projectile.ValueRO.Source,
                        Amount = damage,
                        Element = projectile.ValueRO.Element
                    });

                    if (SystemAPI.HasComponent<ForcedDisplacement>(target.Entity))
                    {
                        float effectiveKnockback = wave.ValueRO.KnockbackDistance * snapshot.ValueRO.EffectStrengthMultiplier;
                        
                        ForcedDisplacement displacement = SystemAPI.GetComponent<ForcedDisplacement>(target.Entity);

                        displacement.Direction = projectile.ValueRO.Direction;
                        displacement.RemainingDistance = effectiveKnockback;
                        displacement.Speed = wave.ValueRO.KnockbackSpeed;
                        SystemAPI.SetComponent(target.Entity, displacement);
                    }
                }

                float3 position = transform.ValueRO.Position;
                position.xy = end;
                transform.ValueRW.Position = position;
            }
        }

        private static bool WasAlreadyHit(DynamicBuffer<ProjectileHitHistory> history, Entity target)
        {
            for (int i = 0; i < history.Length; i++)
            {
                if (history[i].Entity == target)
                    return true;
            }

            return false;
        }
    }
}