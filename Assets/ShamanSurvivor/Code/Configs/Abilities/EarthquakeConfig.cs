using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "Earthquake Config", menuName = "Shaman Survivor/Abilities/Earthquake")]
    public class EarthquakeConfig : AbilityConfig
    {
        [Header("Earthquake")]
        [Min(0.1f)]
        public float Radius = 3f;
        [Min(0.1f)]
        public float Duration = 4f;
        [Min(0.05f)]
        public float TickInterval = 0.5f;
        [Range(0f, 1f)]
        public float SlowAmount = 0.3f;
    }
}