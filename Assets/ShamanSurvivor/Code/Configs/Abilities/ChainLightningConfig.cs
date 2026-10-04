using UnityEngine;

namespace ShamanSurvivor.Configs
{
    [CreateAssetMenu(fileName = "Chain Lightning Config", menuName = "Shaman Survivor/Abilities/Chain Lightning")]
    public class ChainLightningConfig : AbilityConfig
    {
        [Min(0f)] public float JumpRange = 4f;
        [Min(0)] public int BaseJumps = 2;
    }
}