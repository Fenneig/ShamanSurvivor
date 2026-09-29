using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Shaman Survivor/Player Config")]
    public class PlayerConfig : GameConfig
    {
        [Header("Movement")]
        [Min(0f)]
        public float MoveSpeed = 5f;
        [Header("Body")]
        [Min(0.01f)]
        public float HitRadius = 0.4f;
        [Header("Health")]
        [Min(1f)]
        public float MaxHealth = 100f;
        [Header("Progression")]
        [Min(1)]
        public int FirstLevelExperience = 5;
    }
}