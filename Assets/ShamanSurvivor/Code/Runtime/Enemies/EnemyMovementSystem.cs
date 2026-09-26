using ShamanSurvivor.Code.Runtime.Player;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameMovementSystemGroup))]
    [UpdateAfter(typeof(PlayerMoveSystem))]
    public partial struct EnemyMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
            state.RequireForUpdate<EnemyTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
            
            float3 playerPosition = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;
            
            HitRadius playerHitRadius = SystemAPI.GetComponent<HitRadius>(playerEntity);

            var job = new MoveEnemiesJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                TargetPosition = playerPosition.xy,
                TargetRadius = playerHitRadius.Value
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}