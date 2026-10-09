using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ShamanSurvivor.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    [UpdateBefore(typeof(DamageApplySystem))]
    public partial struct EarthquakeZoneSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EarthquakeZone>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();

            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            gridSystem.BuildHandle.Complete();
            var grid = gridSystem.Grid.AsReadOnly();
            
            ComponentLookup<Health> healthLookup = SystemAPI.GetComponentLookup<Health>(true);
            BufferLookup<DamageEvent> damageLookup = SystemAPI.GetBufferLookup<DamageEvent>();
            ComponentLookup<Dead> deadLookup = SystemAPI.GetComponentLookup<Dead>();
            BufferLookup<Slow> slowLookup = SystemAPI.GetBufferLookup<Slow>();
            
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            float deltaTime = SystemAPI.Time.DeltaTime;

            using NativeList<EnemySpatialEntry> targets = new NativeList<EnemySpatialEntry>(Allocator.Temp);

            foreach (var (zone, entity)in SystemAPI.Query<RefRW<EarthquakeZone>>()
                         .WithEntityAccess())
            {
                zone.ValueRW.RemainingDuration -= deltaTime;

                if (zone.ValueRO.RemainingDuration <= 0f)
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }
                
                SpatialQuery.CollectInRadius(grid, zone.ValueRO.Position, zone.ValueRO.Radius, targets);
                Debug.Log($"earthquake collect {targets.Length} targets");
                float tickInterval = math.max(0.01f, zone.ValueRO.TickInterval);

                float tickTimer = zone.ValueRO.TickTimer - deltaTime;

                int damageTicks = 0;

                while (tickTimer <= 0f)
                {
                    damageTicks++;

                    tickTimer += tickInterval;
                }

                zone.ValueRW.TickTimer = tickTimer;
                Debug.Log($"earthquake try damage {damageTicks} times");
                

                for (int i = 0; i < targets.Length; i++)
                {
                    Entity target = targets[i].Entity;

                    if (!deadLookup.HasComponent(target) || deadLookup.IsComponentEnabled(target))
                        continue;

                    if (slowLookup.HasBuffer(target))
                    {
                        DynamicBuffer<Slow> slowEffects = slowLookup[target];
                        
                        SlowUtility.ApplyOrRefresh(slowEffects, target, zone.ValueRO.SlowAmount, deltaTime * 2);
                    }
                    
                    if (damageTicks <= 0)
                        continue;
                    
                    float damage = zone.ValueRO.DamagePerTick * damageTicks;

                    DamageUtility.TryAddDamage(target, ref healthLookup, ref damageLookup, ref deadLookup,
                        new DamageEvent
                        {
                            Source = zone.ValueRO.Source,
                            Amount = damage,
                            Element = zone.ValueRO.DamageElement
                        });
                }

                state.Dependency = default;
            }
        }
    }
}