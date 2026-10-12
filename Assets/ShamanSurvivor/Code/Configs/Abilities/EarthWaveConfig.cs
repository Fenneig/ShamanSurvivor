using ShamanSurvivor.Runtime;
using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "Earth Wave Config", menuName = "Shaman Survivor/Abilities/Earth Wave")]
    public sealed class EarthWaveConfig : ProjectileAbilityConfig
    {
        [Header("Hit Shape")]
        [Min(0.1f)] public float HalfWidth = 2f;
        [Min(0.1f)] public float HalfDepth = 0.5f;
        [Header("Slow")]
        [Range(0f, SlowUtility.UPPER_BOUND)] public float SlowAmount = 0.25f;
        [Min(0.1f)] public float SlowDuration = 2f;
    }
}