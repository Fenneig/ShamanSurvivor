using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "Lightning Shield Config", menuName = "Shaman Survivor/Abilities/Lightning Shield")]
    public sealed class LightningShieldConfig : AbilityConfig
    {
        [Header("Orbit")]
        [Min(0.01f)] public float OrbitRadius = 2f;
        [Min(1)] public int BaseOrbCount = 1;
        [Header("Orb Collision")]
        [Min(0.01f)] public float OrbHitRadius = 0.4f;
    }
}