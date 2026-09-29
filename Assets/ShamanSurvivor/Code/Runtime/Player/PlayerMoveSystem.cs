using ShamanSurvivor.Shared;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Runtime
{
    [UpdateInGroup(typeof(GameMovementSystemGroup))]
    public partial struct PlayerMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (transform, movement, passiveProgresses, input) in 
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovement>, DynamicBuffer<PassiveProgress>, RefRO<PlayerInput>>()
                         .WithAll<PlayerTag>())
            {
                float3 position = transform.ValueRO.Position;
                
                float effectiveMove = movement.ValueRO.Speed * (1f + PassiveProgressUtility.GetBonus(passiveProgresses, GlobalPassiveId.MoveSpeed));
                
                position.xy += input.ValueRO.Move * effectiveMove * deltaTime;
                
                transform.ValueRW.Position = position;
            }
        }
        
    }
}