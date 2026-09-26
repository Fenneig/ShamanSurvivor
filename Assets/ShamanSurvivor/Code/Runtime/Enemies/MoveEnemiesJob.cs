using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime
{
    [BurstCompile]
    [WithAll(typeof(EnemyTag))]
    public partial struct MoveEnemiesJob : IJobEntity
    {
        public float DeltaTime;
        public float2 TargetPosition;
        public float TargetRadius;
        
        private void Execute(ref LocalTransform transform, in EnemyMovement movement, in HitRadius hitRadius)
        {
            float2 position = transform.Position.xy;

            float2 toTarget = TargetPosition - position;
            
            float distanceSq = math.lengthsq(toTarget);

            float stopDistance = TargetRadius + hitRadius.Value;
            
            float stopDistanceSq = stopDistance * stopDistance;
            
            if (distanceSq < stopDistanceSq)
                return;
            
            float distance = math.sqrt(distanceSq);

            float2 direction = toTarget / distance;
            
            float remainingDistance = distance - stopDistance;

            float movementDistance = math.min(movement.Speed * DeltaTime, remainingDistance);

            position = direction * movementDistance;
            
            transform.Position += new float3(position.x, position.y, transform.Position.z);
        }
    }
}