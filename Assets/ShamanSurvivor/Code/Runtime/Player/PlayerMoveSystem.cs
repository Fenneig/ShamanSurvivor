using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ShamanSurvivor.Code.Runtime.Player
{
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

            foreach (var (transform, movement, input) in 
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<PlayerMovement>, RefRO<PlayerInput>>()
                         .WithAll<PlayerTag>())
            {
                float3 position = transform.ValueRO.Position;
                
                position.xy += input.ValueRO.Move * movement.ValueRO.Speed * deltaTime;
                
                transform.ValueRW.Position = position;
            }
        }
        
    }
}