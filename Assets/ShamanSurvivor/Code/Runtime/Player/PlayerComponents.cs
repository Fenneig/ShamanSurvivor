using Unity.Entities;
using Unity.Mathematics;

namespace ShamanSurvivor.Code.Runtime.Player
{
    public struct PlayerTag : IComponentData{}

    public struct PlayerMovement : IComponentData
    {
        public float Speed;
    }

    public struct PlayerInput : IComponentData
    {
        public float2 Move;
    }
}