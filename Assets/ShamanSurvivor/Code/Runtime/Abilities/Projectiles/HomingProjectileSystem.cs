using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameAbilitySystemGroup))]
    [UpdateAfter(typeof(LightningAbilitySystem))]
    public partial struct HomingProjectileSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<ProjectileTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();

            float deltaTime = SystemAPI.Time.DeltaTime;
            
            ComponentLookup<LocalTransform> transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            ComponentLookup<Health> healthLookup = SystemAPI.GetComponentLookup<Health>(true);
            ComponentLookup<HitRadius> hitRadiusLookup = SystemAPI.GetComponentLookup<HitRadius>(true);
            BufferLookup<DamageEvent> damageBufferLookup = SystemAPI.GetBufferLookup<DamageEvent>(false);
            
            EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (transform,
                         projectile,
                         body,
                         projectileEntity) in SystemAPI.Query<
                             RefRW<LocalTransform>, 
                             RefRW<HomingProjectile>, 
                             RefRO<ProjectileBody>>()
                         .WithAll<ProjectileTag>()
                         .WithEntityAccess())
            {
                ref HomingProjectile projectileData = ref projectile.ValueRW;

                projectileData.RemainingLifetime -= deltaTime;

                if (projectileData.RemainingLifetime <= 0f)
                {
                    ecb.DestroyEntity(projectileEntity);
                    
                    continue;
                }

                Entity target = projectileData.Target;

                if (!transformLookup.HasComponent(target) ||
                    !healthLookup.HasComponent(target) ||
                    !hitRadiusLookup.HasComponent(target) ||
                    !damageBufferLookup.HasBuffer(target))
                {
                    ecb.DestroyEntity(projectileEntity);
                    
                    continue;
                }

                if (healthLookup[target].Current <= 0f)
                {
                    ecb.DestroyEntity(projectileEntity);
                    
                    continue;
                }

                float2 currentPosition = transform.ValueRO.Position.xy;
                
                float2 targetPosition = transformLookup[target].Position.xy;
                float2 toTarget = targetPosition - currentPosition;

                float distanceSq = math.lengthsq(toTarget);
                float hitDistance = body.ValueRO.HitRadius + hitRadiusLookup[target].Value;
                float hitDistanceSq = hitDistance * hitDistance;

                if (distanceSq <= hitDistanceSq)
                {
                    ApplyHit(target, projectileEntity, projectileData, damageBufferLookup, ecb);
                    
                    continue;
                }
                
                float distance = math.length(toTarget);
                float moveDistance = projectileData.Speed * deltaTime;
                
                float remainingDistance = distance - moveDistance;

                if (moveDistance >= remainingDistance)
                {
                    ApplyHit(target, projectileEntity, projectileData, damageBufferLookup, ecb);
                    
                    continue;
                }
                
                float2 direction = toTarget / distance;

                currentPosition += direction * moveDistance;
                
                float3 position = transform.ValueRW.Position;
                position.xy = currentPosition;
                
                transform.ValueRW.Position = position;
            }
        }

        private void ApplyHit(
            Entity target, 
            Entity projectileEntity, 
            in HomingProjectile projectileData, 
            BufferLookup<DamageEvent> damageBufferLookup, 
            EntityCommandBuffer ecb)
        {
            DynamicBuffer<DamageEvent> damageBuffer = damageBufferLookup[target];

            damageBuffer.Add(new DamageEvent
            {
                Source = projectileData.Source,
                Amount = projectileData.Damage,
                Element = projectileData.DamageElement
            });
            
            ecb.DestroyEntity(projectileEntity);
        }
    }
}