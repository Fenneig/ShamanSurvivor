using ShamanSurvivor.Shared;
using UnityEngine;

namespace ShamanSurvivor.Configs
{
    public abstract class AbilityConfig : GameConfig
    {
        [Header("Identity")]
        public AbilityId Ability;
        [Header("Projectile")]
        public GameObject ProjectilePrefab;
        [Min(0f)]
        public float Damage = 10f;
        public DamageElement DamageElement;
        [Min(0f)]
        public float Range = 8f;
        [Min(0.01f)]
        public float AttackInterval = 1f;
        [Min(0f)]
        public float ProjectileSpeed = 10f;
        [Min(0.01f)]
        public float ProjectileLifetime = 5f;
    }
}