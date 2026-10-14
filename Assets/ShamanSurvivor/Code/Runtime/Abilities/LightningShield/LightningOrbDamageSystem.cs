using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameCombatSystemGroup))]
    [UpdateBefore(typeof(DamageApplySystem))]
    public partial struct LightningOrbDamageSystem : ISystem
    {
        private const double TWO_PI = Math.PI * 2.0;

        public void OnUpdate(ref SystemState state)
        {
            SystemHandle gridHandle = state.WorldUnmanaged.GetExistingUnmanagedSystem<EnemySpatialGridSystem>();
            ref EnemySpatialGridSystem gridSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<EnemySpatialGridSystem>(gridHandle);
            JobHandle.CombineDependencies(state.Dependency, gridSystem.BuildHandle).Complete();
            var grid = gridSystem.Grid.AsReadOnly();
            ComponentLookup<Health> healthLookup = SystemAPI.GetComponentLookup<Health>(true);
            BufferLookup<DamageEvent> damageLookup = SystemAPI.GetBufferLookup<DamageEvent>();
            ComponentLookup<Dead> deadLookup = SystemAPI.GetComponentLookup<Dead>();

            using NativeList<EnemySpatialEntry> targets = new(Allocator.Temp);

            foreach (var (orbRef, hits)in SystemAPI.Query<RefRO<LightningOrb>, DynamicBuffer<LightningOrbHit>>()
                         .WithAll<LightningOrbActive>())
            {
                LightningOrb orb = orbRef.ValueRO;

                DynamicBuffer<LightningOrbHit> history = hits;

                for (int i = history.Length - 1; i >= 0; i--)
                {
                    if (orb.FromAngle - history[i].LastHitAngle >= TWO_PI || !healthLookup.HasComponent(history[i].Target))
                        history.RemoveAtSwapBack(i);
                }

                double angleDelta = orb.ToAngle - orb.FromAngle;

                int angleSteps = Math.Max(1, (int)Math.Ceiling(angleDelta / 0.15));
                float playerTravel = math.distance(orb.FromCenter, orb.ToCenter);
                int movementSteps = Math.Max(1, (int)Math.Ceiling(playerTravel / math.max(0.5f, orb.HitRadius)));
                int steps = Math.Max(angleSteps, movementSteps);

                for (int step = 0; step < steps; step++)
                {
                    float t0 = step / (float)steps;
                    float t1 = (step + 1) / (float)steps;

                    float2 start = GetPosition(orb, t0);
                    float2 end = GetPosition(orb, t1);

                    SpatialQuery.CollectSegmentHits(grid, start, end, orb.HitRadius, targets);

                    double hitAngle = orb.FromAngle + angleDelta * t1;

                    for (int i = 0; i < targets.Length; i++)
                    {
                        Entity target = targets[i].Entity;

                        if (WasHitRecently(history, target, hitAngle))
                            continue;

                        bool applied = DamageUtility.TryAddDamage(target, ref healthLookup, ref damageLookup, ref deadLookup,
                                new DamageEvent
                                {
                                    Source = orb.Owner,
                                    Amount = orb.Damage,
                                    Element = orb.Element
                                });

                        if (!applied)
                            continue;

                        history.Add(new LightningOrbHit
                        {
                            Target = target,
                            LastHitAngle = hitAngle
                        });
                    }
                }
            }
        }

        private float2 GetPosition(in LightningOrb orb, float t)
        {
            float2 center = math.lerp(orb.FromCenter, orb.ToCenter, t);
            double angle = orb.FromAngle + (orb.ToAngle - orb.FromAngle) * t + TWO_PI * orb.Slot / orb.OrbCount;

            return center + new float2((float)Math.Cos(angle), (float)Math.Sin(angle)) * orb.OrbitRadius;
        }

        private bool WasHitRecently(DynamicBuffer<LightningOrbHit> history, Entity target, double currentAngle)
        {
            for (int i = 0; i < history.Length; i++)
            {
                if (history[i].Target != target)
                    continue;

                return currentAngle - history[i].LastHitAngle < TWO_PI;
            }

            return false;
        }
    }
}