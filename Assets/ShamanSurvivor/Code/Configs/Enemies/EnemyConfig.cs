using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Shaman Survivor/Enemy Config")]
    public class EnemyConfig : GameConfig
    {
        [Header("Movement")]
        [Min(0f)]
        public float MoveSpeed = 2f;
        [Header("Body")]
        [Min(0.01f)]
        public float HitRadius = 0.35f;
        [Header("Health")]
        [Min(1f)]
        public float MaxHealth = 10f;
        [Header("Contact Damage")]
        [Min(0f)]
        public float ContactDamage = 10f;
        [Min(0.01f)]
        public float ContactDamageInterval = 1f;
        [Header("Separation")]
        [Min(0.01f)]
        public float SeparationSearchRadius = 1.25f;
        [Min(0f)]
        public float PersonalSpace = 0.05f;
        [Min(0f)]
        public float SeparationStrength = 8f;
        [Header("Experience")]
        [Min(0)]
        public int ExperienceReward = 1;
    }
}