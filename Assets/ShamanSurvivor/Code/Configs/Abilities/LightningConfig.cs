using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "LightningConfig", menuName = "Shaman Survivor/Abilities/Lightning")]
    public class LightningConfig : GameConfig
    {  
        [Header("Targeting")]
        [Min(0f)]
        public float Range = 8f;
        [Header("Combat")]
        [Min(0f)]
        public float Damage = 10f;
        [Min(0.01f)]
        public float AttackInterval = 0.75f;
        [Header("Projectile")]
        public GameObject ProjectilePrefab;
        [Min(0f)]
        public float ProjectileSpeed = 10f;
        [Min(0.01f)]
        public float ProjectileLifetime = 5f;
    }
}