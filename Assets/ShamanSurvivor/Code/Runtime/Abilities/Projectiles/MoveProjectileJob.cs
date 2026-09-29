using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
{
    [BurstCompile]
    [WithAll(typeof(ProjectileTag))]
    public partial struct MoveProjectileJob : IJobEntity
    {
        [ReadOnly] public NativeParallelMultiHashMap<int2, EnemySpatialEntry>.ReadOnly Grid;
        [ReadOnly] public ComponentLookup<Health> HealthLookup;

        public BufferLookup<DamageEvent> DamageBufferLookup;
        public EntityCommandBuffer ECB;

        public float DeltaTime;

        private void Execute(
            Entity entity,
            ref LocalTransform transform,
            ref Projectile projectile,
            in ProjectileBody body)
        {
            projectile.RemainingLifetime -= DeltaTime;

            if (projectile.RemainingLifetime <= 0f)
            {
                ECB.DestroyEntity(entity);

                return;
            }

            float2 start = transform.Position.xy;
            float2 end = start + projectile.Direction * projectile.Speed * DeltaTime;
            bool hit = SpatialQuery.TryFindFirstDamageableHit(Grid, start, end, body.HitRadius, HealthLookup, DamageBufferLookup, out Entity hitEntity);

            if (hit)
            {
                if (DamageBufferLookup.HasBuffer(hitEntity))
                {
                    DynamicBuffer<DamageEvent> damageBuffer = DamageBufferLookup[hitEntity];

                    damageBuffer.Add(new DamageEvent
                    {
                        Source = projectile.Source,
                        Amount = projectile.Damage,
                        Element = projectile.Element
                    });
                }

                ECB.DestroyEntity(entity);

                return;
            }

            float3 position = transform.Position;
            position.xy = end;
            transform.Position = position;
        }
    }
}