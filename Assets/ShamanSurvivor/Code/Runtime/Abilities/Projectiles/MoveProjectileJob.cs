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
            bool hit = SpatialQuery.TryFindFirstDamageableHit(Grid, start, end, body.HitRadius, HealthLookup, DamageBufferLookup, out float hitT ,out Entity hitEntity);

            if (hit)
            {
                float2 hitPosition = math.lerp(start, end, hitT);

                float3 position = transform.Position;
                position.xy = hitPosition;
                
                ECB.AddComponent(entity, new ProjectileHitEvent
                {
                    Target = hitEntity,
                    Position = hitPosition
                });

                return;
            }

            float3 newPosition = transform.Position;
            newPosition.xy = end;
            transform.Position = newPosition;
        }
    }
}