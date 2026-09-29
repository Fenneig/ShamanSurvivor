using Unity.Entities;

namespace ShamanSurvivor.Runtime
{
    public struct ExperienceReward : IComponentData
    {
        public int Value;
    }

    public struct ExperienceOrb : IComponentData
    {
        public int Value;

        public float MagnetRadius;
        public float CollectRadius;
        public float MoveSpeed;
    }

    public struct PlayerExperience : IComponentData
    {
        public int Level;

        public int Current;
        public int Required;

        public int PendingLevelUps;
    }
}