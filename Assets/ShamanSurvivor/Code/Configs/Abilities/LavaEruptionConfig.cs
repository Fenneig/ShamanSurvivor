using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "Lava Eruption Config", menuName = "Shaman Survivor/Abilities/Lava Eruption")]
    public class LavaEruptionConfig : ProjectileAbilityConfig
    {
        [Min(1f)] 
        public float BurningTargetDamageMultiplier = 1.5f;
        [Min(0.01f)] 
        public float ExplosionRadius = 1.5f;
        [Header("Burning")]
        [Min(0)] 
        public float BurnDamagePerTick = 3f;
        [Min(0.01f)] 
        public float BurnTickInterval = .5f;
        [Min(0)]
        public float BurnDuration = 3f;
    }
}