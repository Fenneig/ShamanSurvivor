using UnityEngine;

namespace ShamanSurvivor.Configs
{
    public abstract class ProjectileAbilityConfig : AbilityConfig
    {
        [Header("Projectile")]
        public GameObject ProjectilePrefab;
        [Min(0f)]
        public float ProjectileSpeed = 10f;
        [Min(0.01f)]
        public float ProjectileLifetime = 5f;
    }
}