using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "Water Wave Config", menuName = "Shaman Survivor/Abilities/Water Wave")]
    public class WaterWaveConfig : ProjectileAbilityConfig
    {
        [Min(0f)]
        [SerializeField] public float KnockbackDistance;
        [Min(.1f)]
        [SerializeField] public float HalfWidth;
        [Min(.1f)]
        [SerializeField] public float HalfDepth;
    }
}