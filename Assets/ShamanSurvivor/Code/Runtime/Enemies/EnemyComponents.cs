using Unity.Entities;

namespace ShamanSurvivor.Code.Runtime
{
    public struct EnemyTag : IComponentData
    {
    }

    public struct EnemyMovement : IComponentData
    {
        public float Speed;
    }

    public struct ContactDamage : IComponentData
    {
        public float Damage;
        public float Interval;
        public float CooldownRemaining;
    }

    public struct EnemySeparation : IComponentData
    {
        public float SearchRadius;
        public float PersonalSpace;
        public float Strength;
    }
}