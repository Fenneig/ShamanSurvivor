using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
{
    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct MoveEnemiesJob : IJobEntity
    {
        public float DeltaTime;
        public float2 TargetPosition;
        public float TargetRadius;
        
        private void Execute(Entity entity, ref LocalTransform transform, in EnemyMovement movement, in HitRadius hitRadius, DynamicBuffer<Slow> slowEffects)
        {
            float2 position = transform.Position.xy;
            float2 toTarget = TargetPosition - position;
            
            float distanceSq = math.lengthsq(toTarget);
            float stopDistance = TargetRadius + hitRadius.Value;
            float stopDistanceSq = stopDistance * stopDistance;
            
            if (distanceSq < stopDistanceSq)
                return;
            
            float slowAmount = 0f;
            
            slowAmount = SlowUtility.GetStrongest(slowEffects);
            
            float distance = math.sqrt(distanceSq);
            float2 direction = toTarget / distance;
            float remainingDistance = distance - stopDistance;
            float effectiveSpeed = movement.Speed * (1f - slowAmount);
            float movementDistance = math.min(effectiveSpeed * DeltaTime, remainingDistance);
            position = direction * movementDistance;
            transform.Position += new float3(position.x, position.y, transform.Position.z);
        }
    }
}