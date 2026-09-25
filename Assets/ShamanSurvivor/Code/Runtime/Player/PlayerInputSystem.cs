using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.InputSystem;

namespace ShamanSurvivor.Code.Runtime.Player
{
    [UpdateInGroup(typeof(GameInputSystemGroup))]
    public partial class PlayerInputSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float2 move = float2.zero;

            ReadKeyboard(ref move);

            move = math.normalizesafe(move);

            foreach (var playerInput in SystemAPI.Query<RefRW<PlayerInput>>()
                         .WithAll<PlayerTag>()) 
                playerInput.ValueRW.Move = move;
        }

        private void ReadKeyboard(ref float2 move)
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
                return;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                move.x -= 1f;

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                move.x += 1f;

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                move.y -= 1f;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                move.y += 1f;
            
        }
    }
}